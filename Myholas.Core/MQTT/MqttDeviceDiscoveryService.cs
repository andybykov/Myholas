using MQTTnet;
using Myholas.Core.Dtos.ESPDevices;
using Myholas.Core.Interfaces;
using System.Collections.Concurrent;
using System.ComponentModel;
using System.Text;
using System.Text.Json;

public sealed class MqttDeviceDiscoveryService : IAsyncDisposable
{
    private readonly IMqttService _mqtt;

    private readonly string _mqttServer;

    private ConcurrentDictionary<string, EspDeviceDto> _espDevices = new();

    private readonly List<string> _discoverTopics = ["esphome/discover/#", "#"];


    public event Action<string, BaseEntityConfigDto>? EntityConfigReceived;

    // сигнатура событий: deviceId, entityId, payload
    public event Action<string, string, string>? StateReceived;

    public event Action<string, string, string>? CommandReceived;

    public event Action<EspDeviceDto>? DeviceUpdated;

    public event Action<EspDeviceDto>? DeviceStatusUpdated;



    public MqttDeviceDiscoveryService(IMqttService mqtt, string mqttServer, string mqttPort = "1883")
    {
        _mqtt = mqtt;
        _mqttServer = mqttServer;
    }

    public async Task StartAsync()
    {
        if (!_mqtt.IsConnected)
        {
            await _mqtt.ConnectAsync(_mqttServer);

            foreach (var topic in _discoverTopics)
            {
                await _mqtt.SubscribeAsync(topic);
                _mqtt.OnMessageReceived += OnMqttMessage;
            }
        }

    }

    // вызов обработчиков в завимисости от топика
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

        //топик похож на ESPHome 
        if (IsEspHomeTopicStructure(topic))
        {
            ProcessEspState(topic, payload);
            ProcessEspCommand(topic, payload);
        }

        if (IsStatusTopic(topic)) {

            ProcessEspStatus(topic, payload);
        }
    }

    // Проверяем структуру топика
    private bool IsEspHomeTopicStructure(string topic)
    {
        var parts = topic.Split('/');
        // device/component/object/state 
        return parts.Length >= 4;
    }

    private bool IsStatusTopic(string topic)
    {
        var parts = topic.Split('/');

        var res = parts[1] == "status";

        return res;

    }
    // обработчик статусов Online/Offline
    private void ProcessEspStatus(string topic, string payload)
    {
        var parts = topic.Split('/');
        var deviceId = parts[0];

        var status = payload.Contains("online") ? true : false;

        // устройство уже есть
        if (_espDevices.TryGetValue(deviceId, out var device))
        {
            device.IsOnline = status;
            DeviceStatusUpdated?.Invoke(device);
        }
        else
        {
            // устройства нет 
            var newDevice = new EspDeviceDto
            {
                Name = deviceId,
                IsOnline = status
            };
            _espDevices.TryAdd(deviceId, newDevice);
            DeviceStatusUpdated?.Invoke(newDevice);
        }
    }

    // обработчик состояний
    private void ProcessEspState(string topic, string payload)
    {
        var parts = topic.Split('/');
        if (parts.Length < 4)
            return;

        string deviceName = parts[0]; // esp-lamp01
        string component = parts[1];
        string objectId = parts[2];
        string action = parts[3].ToLower();

        if (!action.StartsWith("state"))
            return;

        string entityId = $"{component}.{objectId}";

        // ПЕРЕДАЕМ deviceName!
        StateReceived?.Invoke(deviceName, entityId, payload);
    }

    // обработчик команд
    private void ProcessEspCommand(string topic, string payload)
    {
        var parts = topic.Split('/');
        if (parts.Length < 4) return;

        string deviceName = parts[0];
        string component = parts[1];
        string objectId = parts[2];
        string action = parts[3].ToLower();

        if (!action.StartsWith("command"))
            return;

        string entityId = $"{component}.{objectId}";
        CommandReceived?.Invoke(deviceName, entityId, payload);
    }

    // обработчик обнаруженных устройств
    private void ProcessEspDiscover(string topic, string payload)
    {
        var dto = JsonSerializer.Deserialize<EspDeviceDto>(payload);
        if (dto?.Name == null)
            return;


        EspDeviceDto espDevice = _espDevices.AddOrUpdate(
                    dto.Name,
                    dto,
                    (_, existing) => {
                        existing.Ip = dto.Ip;
                        existing.FriendlyName = dto.FriendlyName;
                        existing.Version = dto.Version;
                        return existing;
                    });

        DeviceUpdated?.Invoke(espDevice);
    }

    // обработчик конфигураций
    private void ProcessEspConfig(string topic, string payload)
    {
        var parts = topic.Split('/', StringSplitOptions.RemoveEmptyEntries); // StringSplitOptions.RemoveEmptyEntries удаляет пустые элементы
        if (parts.Length < 4) 
            return;

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

        if (config == null) 
            return;

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
        // отписка от топиков
        foreach (var topic in _discoverTopics) 
            await _mqtt.UnsubscribeAsync(topic);
    }

    public async ValueTask DisposeAsync() => 
        await StopAsync();
}
