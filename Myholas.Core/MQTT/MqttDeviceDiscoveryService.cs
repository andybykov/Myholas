using MQTTnet;
using Myholas.Core.Dtos.ESPDevices;
using Myholas.Core.Interfaces;
using System.Collections.Concurrent;
using System.Text;
using System.Text.Json;
using static Microsoft.EntityFrameworkCore.DbLoggerCategory.Database;


/// <summary>
/// Сервис для обнаружения MQTT устройств (ESPHome и Home Assistant MQTT Discovery)
/// Подписывается на топики, парсит в EspDeviceDto конфигурации и состояния устройств
/// Генерирует события для MqttToEventBusBridge: EntityConfigReceived, StateReceived, DeviceUpdated
/// </summary>
public sealed class MqttDeviceDiscoveryService : IAsyncDisposable
{

    private readonly IMqttService _mqtt;

    private readonly string _mqttServer;

    // Словарь ESPHome устройств с конфигурациями BaseEntityConfigDto
    private readonly ConcurrentDictionary<string, EspDeviceDto> _espDevices = new();
   
    //private readonly ConcurrentDictionary<string, BaseEntityConfigDto> _entityConfigs = new();

    //private readonly ConcurrentDictionary<string, int> _deviceEntityHash = new();

    // Топики обнаружения
    private readonly List<string> _discoverTopics =
        [
        "esphome/discover/#", //  ESPHome
        "#" // все остальные топики 
        ];     

    // События
    public event Action<BaseEntityConfigDto>? EntityConfigReceived; // конфигруация 

    public event Action<string, string>? StateReceived; // (entityId, state) 

    public event Action<string, string>? CommandReceived; // (entityId, state) 

    public event Action<EspDeviceDto>? DeviceUpdated; // устройство обновлено 




    public MqttDeviceDiscoveryService(IMqttService mqtt, string mqttServer, string mqttPort = "1883")
    {
        _mqtt = mqtt;
        _mqttServer = mqttServer;
    }

    // Подключение к MQTT, подписка на топики, регистрация обработчика
    public async Task StartAsync()
    {
        if (!_mqtt.IsConnected)
            await _mqtt.ConnectAsync(_mqttServer);

        // подписка на топики
        foreach (var topic in _discoverTopics)
        {
            await _mqtt.SubscribeAsync(topic);
        }

        // обработчик входящих MQTT сообщений
        _mqtt.OnMessageReceived += OnMqttMessage;
    }

    // Обработчик ВСЕХ входящих MQTT сообщений
    // Определяет тип сообщения и направляет в соответствующий метод обработки
    private async Task OnMqttMessage(MqttApplicationMessageReceivedEventArgs args)
    {
        // Извлекаем topic, payload
        var topic = args.ApplicationMessage.Topic;
        var payload = Encoding.UTF8.GetString(args.ApplicationMessage.Payload).Trim();

        // ESPHome devices detected
        if (topic.StartsWith("esphome/discover/"))
        {
            ProcessEspDiscover(topic, payload);

            return;
        }

        // Home Assistant MQTT Discovery
        if (topic.StartsWith("homeassistant/") && topic.EndsWith("/config"))
        {
            ProcessEspConfig(topic, payload);

            return;
        }

        // Обработка state/command топиков ESPHome
        if (IsEspHomeTopic(topic))
        {
            ProcessEspState(topic, payload);
            ProcessEspCommand(topic, payload);

            return;
        }
        Console.WriteLine($"[MQTT DEVICE DISCOVERY SERVICE] Unknown topic {topic}");
        return;
    }


    // Проверяет принадлежит ли топик известному ESPHome устройству
    private bool IsEspHomeTopic(string topic)
    {
        // Формат топика ESPHome: <device_name>/<component>/<object_id>/<state|command>
        // esp-lamp01/switch/lamp01/state
        var parts = topic.Split('/');

        if (parts.Length < 2) 
            return false;

        string deviceName = parts[0];

        // есть ли устройство в словаре
        return _espDevices.ContainsKey(deviceName);
    }

    // Обработка state топиков ESPHome устройств
    private void ProcessEspState(string topic, string payload)
    {
        // Разбираем топик на части
        var parts = topic.Split('/');

        if (parts.Length < 3) 

            return;

        string deviceName = parts[0];

        if (!_espDevices.ContainsKey(deviceName)) 

            return; // содержит ли словарь заданный ключ

        string component = parts[1];
        string objectId = parts[2];
        string state = parts[3].ToLower();

        // Это состояние??
        if (!state.StartsWith("state"))

            return;

        string entityId = $"{component}.{objectId}";

        StateReceived?.Invoke(entityId, payload); // вызываем все методы которые подписаны на StateReceived
    }

    // Обработка command топиков ESPHome устройств
    private void ProcessEspCommand(string topic, string payload)
    {
        // Разбираем топик на части
        var parts = topic.Split('/');

        if (parts.Length < 3)

            return;

        string deviceName = parts[0];

        if (!_espDevices.ContainsKey(deviceName))

           return; // содержит ли словарь заданный ключ

        string component = parts[1];
        string objectId = parts[2];
        string command = parts[3].ToLower();

        // Это команда?
        if (!command.StartsWith("command"))

            return;
        
        string entityId = $"{component}.{objectId}";

        CommandReceived?.Invoke(entityId, payload); // вызываем все методы которые подписаны на CommandReceived
    }

    // Парсинг устройств ESPHome 
    private void ProcessEspDiscover(string topic, string payload)
    {
        // JSON в DTO 
        var dto = JsonSerializer.Deserialize<EspDeviceDto>(payload);

        Console.WriteLine($"[MQTT DEVICE DISCOVERY SERVICE] Received discover totic: {topic}, {payload}");

        if (dto?.Name == null)
        {
            Console.WriteLine($"[MQTT DEVICE DISCOVERY SERVICE] NULL DTO");
            return;
        }
        Console.WriteLine($"[MQTT DEVICE DISCOVERY SERVICE] DTO Name: {dto.Name}");


        // Обновляем устройство или создаем
        EspDeviceDto device = _espDevices.AddOrUpdate(
            key: dto.Name,
            addValue: dto,
            updateValueFactory: (_, existing) =>
            {
                existing.Ip = dto.Ip;
                existing.Name = dto.Name;
                existing.FriendlyName = dto.FriendlyName;
                existing.Version = dto.Version;
                existing.Mac = dto.Mac;
                existing.Platform = dto.Platform;
                existing.Board = dto.Board;
                existing.Network = dto.Network;

                return existing;
            });
        Console.WriteLine($"[MQTT DEVICE DISCOVERY SERVICE] Device added/updated: {device.Name}");

        int subscribers = DeviceUpdated?.GetInvocationList().Length ?? 0;
        Console.WriteLine($"[MQTT DEVICE DISCOVERY SERVICE] DeviceUpdated has {subscribers} subscribers");
        DeviceUpdated?.Invoke(device); // вызываем событи
    }

    // Парсинг конфигураций устройств
    private void ProcessEspConfig(string topic, string payload)
    {
       
        Console.WriteLine($"[MQTT DEVICE DISCOVERY SERVICE] Received config, topic: {topic}");
        var parts = topic.Split('/', StringSplitOptions.RemoveEmptyEntries);
        if (parts.Length != 5) return;

        string domain = parts[1];
        string deviceId = parts[2];
        string objectId = parts[3];


        BaseEntityConfigDto? config;

        switch (domain)
        {
            case "sensor":
                // Десериализуем как датчик
                config = JsonSerializer.Deserialize<SensorEntityConfigDto>(payload);
                break;

            case "switch":
                // выключатель
                config = JsonSerializer.Deserialize<SwitchEntityConfigDto>(payload);
                break;

            case "select":
                // селектор 
                config = JsonSerializer.Deserialize<SelectEntityConfigDto>(payload);
              //  Console.WriteLine($"SELCET: {payload}");
                break;

            case "light":
                // светильник 
                config = JsonSerializer.Deserialize<LightEntityConfigDto>(payload);
                break;

            default:
                // хз что, базовый DTO
                config= JsonSerializer.Deserialize<BaseEntityConfigDto>(payload);
                break;
        }

        if (config == null) 
            return;

        config.Domain = domain;
        config.ObjectId = objectId;

        // Получаем / создаем
        var device = _espDevices.GetOrAdd(deviceId, _ => new EspDeviceDto { Name = deviceId });

        // Добавляем конфигурацию
        if (!device.Entities.Any(e => e.UniqueId == config.UniqueId))
            device.Entities.Add(config);

        EntityConfigReceived?.Invoke(config);
        DeviceUpdated?.Invoke(device);
    }

    // Возвращает все обнаруженные ESPHome устройства
    public IReadOnlyDictionary<string, EspDeviceDto> GetDiscoveredDevices()
        => _espDevices;


    // Возвращает все конфигурации сущностей 
   //  public IReadOnlyDictionary<string, BaseEntityConfigDto> GetEntityConfigs()
   //      => _entityConfigs;

    // Остановка сервиса
    public async Task StopAsync()
    {
        _mqtt.OnMessageReceived -= OnMqttMessage; // отписка
        foreach (var topic in _discoverTopics)
        {
            await _mqtt.UnsubscribeAsync(topic);
        }
    }

    // Асинхронная очистка ресурсов 
    public async ValueTask DisposeAsync() => await StopAsync();
}