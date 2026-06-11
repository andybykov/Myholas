using Myholas.Core.Interfaces;
using System.Text.Json;

namespace Myholas.Core.MQTT
{
    // Мост MQTT Discovery и EventBus
    // Публикует события в EventBus на основе событий из IMqttDiscoveryService
    // Теперь Bridge универсален для любого типа устройств и конфигов
    public class MqttToEventBusBridge<TDevice, TConfig>
    {
        private readonly IEventBus _eventBus;
        private readonly IMqttDiscoveryService<TDevice, TConfig> _discoveryService;

        public MqttToEventBusBridge(
            IEventBus eventBus,
            IMqttDiscoveryService<TDevice, TConfig> discoveryService)
        {
            _eventBus = eventBus;
            _discoveryService = discoveryService;

            // Подписка на универсальные события от discovery сервиса
            _discoveryService.StateReceived += OnStateReceived;
            _discoveryService.CommandReceived += OnCommandReceived;
            _discoveryService.DeviceUpdated += OnDeviceUpdated;
            _discoveryService.DeviceStatusUpdated += OnDeviceStatusUpdated;
            _discoveryService.EntityConfigReceived += OnConfigReceived;
        }

        // Обработчик конфигурации устройства (TConfig)
        private void OnConfigReceived(string deviceId, TConfig config)
        {
            var recived = "config.received";
            // Передаем оба параметра в JSON, чтобы менеджер их увидел
            var data = JsonSerializer.Serialize(new { DeviceId = deviceId, Config = config });

            _eventBus.Emit(recived, data);
        }

        // Обработчик обновления устройства (TDevice)
        private void OnDeviceUpdated(TDevice device)
        {
            var updated = "device.updated";
            var json = JsonSerializer.Serialize(device);

#if DEB
            Console.WriteLine($"[BridgeToBus] Emitting device.updated for {device}");
#endif
            _eventBus.Emit(updated, json);
        }

        // Обработчик статуса устройства (TDevice)
        private void OnDeviceStatusUpdated(TDevice device)
        {
            var statusChanged = "device_status.updated";
            var json = JsonSerializer.Serialize(device);

#if DEB
            Console.WriteLine($"[BridgeToBus] Emitting status update for {device}");
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
