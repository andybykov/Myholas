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
            _discoveryService.StateReceived += OnStateReceived;
            _discoveryService.CommandReceived += OnCommandReceived;
            _discoveryService.DeviceUpdated += OnDeviceUpdated;
            _discoveryService.DeviceStatusUpdated += OnDeviceStatusUpdated;
            _discoveryService.EntityConfigReceived += OnConfigRecived;
        }

        // Обработчик BaseEntityConfigDto ESPHome устройства
        private void OnConfigRecived(string deviceId, BaseEntityConfigDto config)
        {
            var recived = "config.received";
            // Передаем оба параметра в JSON, чтобы менеджер их увидел
            var data = JsonSerializer.Serialize(new { DeviceId = deviceId, Config = config });

            _eventBus.Emit(recived, data);
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

        // Обработчик статуса EspDeviceDto ESPHome устройства
        private void OnDeviceStatusUpdated(EspDeviceDto device)
        {
            var statusChanged = "device_status.updated"; 
            var json = JsonSerializer.Serialize(device);

#if DEB
            Console.WriteLine($"[BridgeToBus] Emitting status update for {device.Name}: {device.IsOnline}");
#endif
            _eventBus.Emit(statusChanged, json);
        }


        // Обработчик изменения состояния 
        private void OnStateReceived(string deviceId, string entityId, string state)
        {
            var changed = "state_changed";
            // Передаем через разделитель | чтобы StateManager мог распарсить
            var data = $"{deviceId}|{entityId}:{state}";

            _eventBus.Emit(changed, data);
        }

        // Обработчик получения команды
        private void OnCommandReceived(string deviceId, string entityId, string state)
        {
            var msg = "command.recived";
            var data = $"{deviceId}|{entityId}:{state}";

            _eventBus.Emit(msg, data);
        }
    }
}