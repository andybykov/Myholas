using MQTTnet;
using Myholas.Core.Dtos.ESPDevices;
using Myholas.Core.Interfaces;
using System.Collections.Concurrent;
using System.Text;
using System.Text.Json;

public sealed class MqttDeviceDiscoveryService : IAsyncDisposable
{
    private readonly IMqttService _mqtt;
    private readonly string _mqttServer;
    private readonly ConcurrentDictionary<string, EspDeviceDto> _espDevices = new();

    private readonly List<string> _discoverTopics = ["esphome/discover/#", "#"];

    public event Action<string, BaseEntityConfigDto>? EntityConfigReceived; // Добавляем string deviceId
    // Изменяем сигнатуры событий: теперь (deviceId, entityId, payload)
    public event Action<string, string, string>? StateReceived;
    public event Action<string, string, string>? CommandReceived;
    public event Action<EspDeviceDto>? DeviceUpdated;

    public MqttDeviceDiscoveryService(IMqttService mqtt, string mqttServer, string mqttPort = "1883")
    {
        _mqtt = mqtt;
        _mqttServer = mqttServer;
    }

    public async Task StartAsync()
    {
        if (!_mqtt.IsConnected) await _mqtt.ConnectAsync(_mqttServer);
        foreach (var topic in _discoverTopics) await _mqtt.SubscribeAsync(topic);
        _mqtt.OnMessageReceived += OnMqttMessage;
    }

    private async Task OnMqttMessage(MqttApplicationMessageReceivedEventArgs args)
    {
        var topic = args.ApplicationMessage.Topic;
        var payload = Encoding.UTF8.GetString(args.ApplicationMessage.Payload).Trim();

        if (topic.StartsWith("esphome/discover/"))
        {
            ProcessEspDiscover(topic, payload);
            return;
        }

        if (topic.StartsWith("homeassistant/") && topic.EndsWith("/config"))
        {
            ProcessEspConfig(topic, payload);
            return;
        }

        // Упрощаем: если топик похож на ESPHome (имеет структуру), обрабатываем его
        if (IsEspHomeTopicStructure(topic))
        {
            ProcessEspState(topic, payload);
            ProcessEspCommand(topic, payload);
        }
    }

    // Проверяем структуру топика, а не наличие в словаре!
    private bool IsEspHomeTopicStructure(string topic)
    {
        var parts = topic.Split('/');
        // Минимум: device/component/object/state (4 части)
        return parts.Length >= 4;
    }

    private void ProcessEspState(string topic, string payload)
    {
        var parts = topic.Split('/');
        if (parts.Length < 4) return;

        string deviceName = parts[0]; // "esp-lamp01" <-- РЕАЛЬНЫЙ ID
        string component = parts[1];
        string objectId = parts[2];
        string action = parts[3].ToLower();

        if (!action.StartsWith("state")) return;

        string entityId = $"{component}.{objectId}";

        // ПЕРЕДАЕМ deviceName!
        StateReceived?.Invoke(deviceName, entityId, payload);
    }

    private void ProcessEspCommand(string topic, string payload)
    {
        var parts = topic.Split('/');
        if (parts.Length < 4) return;

        string deviceName = parts[0];
        string component = parts[1];
        string objectId = parts[2];
        string action = parts[3].ToLower();

        if (!action.StartsWith("command")) return;

        string entityId = $"{component}.{objectId}";
        CommandReceived?.Invoke(deviceName, entityId, payload);
    }

    private void ProcessEspDiscover(string topic, string payload)
    {
        var dto = JsonSerializer.Deserialize<EspDeviceDto>(payload);
        if (dto?.Name == null) return;

        EspDeviceDto device = _espDevices.AddOrUpdate(
            dto.Name,
            dto,
            (_, existing) => {
                existing.Ip = dto.Ip;
                existing.FriendlyName = dto.FriendlyName;
                existing.Version = dto.Version;
                return existing;
            });

        DeviceUpdated?.Invoke(device);
    }

    private void ProcessEspConfig(string topic, string payload)
    {
        var parts = topic.Split('/', StringSplitOptions.RemoveEmptyEntries);
        if (parts.Length < 4) return;

        string domain = parts[1];
        string deviceId = parts[2];
        string objectId = parts[3];

        BaseEntityConfigDto? config = domain switch
        {
            "sensor" => JsonSerializer.Deserialize<SensorEntityConfigDto>(payload),
            "switch" => JsonSerializer.Deserialize<SwitchEntityConfigDto>(payload),
            "select" => JsonSerializer.Deserialize<SelectEntityConfigDto>(payload),
            "light" => JsonSerializer.Deserialize<LightEntityConfigDto>(payload),
            _ => JsonSerializer.Deserialize<BaseEntityConfigDto>(payload)
        };

        if (config == null) return;

        config.Domain = domain;
        config.ObjectId = objectId;

        var device = _espDevices.GetOrAdd(deviceId, _ => new EspDeviceDto { Name = deviceId });
        if (!device.Entities.Any(e => e.UniqueId == config.UniqueId))
            device.Entities.Add(config);

        EntityConfigReceived?.Invoke(deviceId, config); // Передаем deviceI
        DeviceUpdated?.Invoke(device);
    }

    public async Task StopAsync()
    {
        _mqtt.OnMessageReceived -= OnMqttMessage;
        foreach (var topic in _discoverTopics) await _mqtt.UnsubscribeAsync(topic);
    }

    public async ValueTask DisposeAsync() => await StopAsync();
}
