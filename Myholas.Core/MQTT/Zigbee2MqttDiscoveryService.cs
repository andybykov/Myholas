using MQTTnet;
using Myholas.Core.Dtos.DeserializationDtos.ESPDevices;
using Myholas.Core.Dtos.DeserializationDtos.Z2mDevices;
using Myholas.Core.Interfaces;
using System.Collections.Concurrent;
using System.Text;
using System.Text.Json;

namespace Myholas.Core.MQTT
{
    public sealed class Zigbee2MqttDiscoveryService : IMqttDiscoveryService<Z2MDeviceDto, Z2MExposeDto>
    {
        private readonly IMqttService _mqtt;
        private readonly string _mqttServer;

        private ConcurrentDictionary<string, Z2MDeviceDto> _zigbeeDevices = new();
        private readonly List<string> _discoverTopics = ["zigbee2mqtt/#"];

        // --- ИСПРАВЛЕНО: Сигнатуры теперь СТРОГО соответствуют интерфейсу ---

        // Было: Action<Z2MExposeDto>, стало: Action<string, Z2MExposeDto>
        public event Action<string, Z2MExposeDto>? EntityConfigReceived;

        public event Action<string, string, string>? StateReceived;
        public event Action<string, string, string>? CommandReceived;
        public event Action<Z2MDeviceDto>? DeviceUpdated;
        public event Action<Z2MDeviceDto>? DeviceStatusUpdated;

        public Zigbee2MqttDiscoveryService(IMqttService mqtt, string mqttServer, string mqttPort = "1883")
        {
            _mqtt = mqtt;
            _mqttServer = mqttServer;
        }

        // УДАЛЕН блок с явной реализацией IMqttDiscoveryService... { throw new NotImplementedException(); }
        // Он здесь не нужен и вреден.

        public async Task StartAsync()
        {
            if (!_mqtt.IsConnected)
            {
                await _mqtt.ConnectAsync(_mqttServer);
                foreach (var topic in _discoverTopics)
                {
                    await _mqtt.SubscribeAsync(topic);
                }
                _mqtt.OnMessageReceived += OnMqttMessage;
            }
        }

        private async Task OnMqttMessage(MqttApplicationMessageReceivedEventArgs args)
        {
            var topic = args.ApplicationMessage.Topic;
            var payload = Encoding.UTF8.GetString(args.ApplicationMessage.Payload).Trim();

            if (topic == "zigbee2mqtt/bridge/devices")
            {
                ProcessZ2MDevicesList(payload);
                return;
            }

            if (topic.EndsWith("/availability"))
            {
                ProcessZ2MStatus(topic, payload);
                return;
            }

            if (topic.EndsWith("/set"))
            {
                ProcessZ2MCommand(topic, payload);
                return;
            }

            if (topic.StartsWith("zigbee2mqtt/") && !topic.Contains("/bridge"))
            {
                ProcessZ2MDeviceState(topic, payload);
            }
        }

        private void ProcessZ2MDevicesList(string payload)
        {
            try
            {
                var devicesInfo = JsonSerializer.Deserialize<List<Z2MDeviceDto>>(payload);
                if (devicesInfo == null) 
                    return;

                foreach (var info in devicesInfo)
                {
                    _zigbeeDevices.AddOrUpdate(info.FriendlyName, info, (_, existing) => info);

                    if (info.Exposes != null)
                    {
                        foreach (var exp in info.Exposes)
                        {
                            // Теперь передаем два параметра: имя устройства и конфиг
                            EntityConfigReceived?.Invoke(info.FriendlyName, exp);
                        }
                    }
                    DeviceUpdated?.Invoke(info);
                }
            }
            catch (JsonException ex)
            {
                Console.WriteLine($"[Z2M Error] Parsing devices list: {ex.Message}");
            }
        }

        private void ProcessZ2MDeviceState(string topic, string payload)
        {
            var parts = topic.Split('/');
            if (parts.Length < 2) return;
            string deviceName = parts[1];

            if (string.IsNullOrWhiteSpace(payload) || !payload.StartsWith("{")) return;

            try
            {
                var data = JsonSerializer.Deserialize<Dictionary<string, object>>(payload);
                if (data == null) return;

                foreach (var item in data)
                {
                    StateReceived?.Invoke(deviceName, item.Key, item.Value?.ToString() ?? "");
                }
            }
            catch (JsonException ex)
            {
                Console.WriteLine($"[Z2M Error] Parsing state {topic}: {ex.Message}");
            }
        }

        private void ProcessZ2MStatus(string topic, string payload)
        {
            var parts = topic.Split('/');
            if (parts.Length < 2) return;
            string deviceName = parts[1];

            bool isOnline = payload.Equals("online", StringComparison.OrdinalIgnoreCase);

            var device = _zigbeeDevices.AddOrUpdate(deviceName,
                name => new Z2MDeviceDto { FriendlyName = name, IsOnline = isOnline },
                (_, existing) => {
                    existing.IsOnline = isOnline;
                    return existing;
                });

            DeviceStatusUpdated?.Invoke(device);
        }

        private void ProcessZ2MCommand(string topic, string payload)
        {
            var parts = topic.Split('/');
            if (parts.Length < 2) return;
            string deviceName = parts[1];
            CommandReceived?.Invoke(deviceName, "set", payload);
        }

        public async Task StopAsync()
        {
            _mqtt.OnMessageReceived -= OnMqttMessage;
            foreach (var topic in _discoverTopics)
            {
                await _mqtt.UnsubscribeAsync(topic);
            }
        }

        public async ValueTask DisposeAsync() => await StopAsync();
    }
}
