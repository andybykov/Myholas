#define DEB
using Myholas.Core.Dtos.ESPDevices;
using Myholas.Core.Interfaces;
using System.Text.Json;

namespace Myholas.Core.MQTT
{
    // Мост MQTT Discovery и EventBus
    // Публикует события в EventBus на основе событий из MqttDeviceDiscoveryService
    public class MqttToEventBusBridge
    {
        private readonly IEventBus _eventBus;

        private readonly MqttDeviceDiscoveryService _discoveryService;

        public MqttToEventBusBridge(IEventBus eventBus, MqttDeviceDiscoveryService discoveryService)
        {
            _eventBus = eventBus;
            _discoveryService = discoveryService;

            // Подписка на события от discovery сервиса
            _discoveryService.DeviceUpdated += OnDeviceUpdated;
            _discoveryService.StateReceived += OnStateReceived;
            _discoveryService.CommandReceived += OnCommandReceived;
            _discoveryService.EntityConfigReceived += OnConfigRecived;

        }

        // Обработчик BaseEntityConfigDto ESPHome устройства
        private void OnConfigRecived (BaseEntityConfigDto config)
        {
            var recived = "config.recived";
            var json = JsonSerializer.Serialize (config);
#if DEB
            Console.WriteLine($"[BridgeToBus] Emitting config.recived for {config.Name}");
#endif
            _eventBus.Emit(recived, json);   

        }

        // Обработчик EspDeviceDto ESPHome устройства
        private void OnDeviceUpdated(EspDeviceDto device)
        {
            var updated = "device.updated";
            var json = JsonSerializer.Serialize(device);

#if DEB
            Console.WriteLine($"[BridgeToBus] Emitting device.updated for {device.Name}");
#endif
            _eventBus.Emit(updated, json);
        }

        // Обработчик изменения состояния 
        private void OnStateReceived(string entityId, string state)
        {
            var changed = "state_changed";
            var st = $"{entityId}:{state}";

            var line = $"{changed},{st}";
#if DEB
            Console.WriteLine($"[BridgeToBus] : {line}");
#endif
            _eventBus.Emit(changed, st);
        }

        // Обработчик получения команды
        private void OnCommandReceived(string entityId, string state)
        {
            var msg = "command.recived";
            var st = $"{entityId}:{state}";

            var line = $"{msg},{st}";
#if DEB
            Console.WriteLine($"[BridgeToBus] : {line}");
#endif
            _eventBus.Emit(msg, st);
        }
    }
}