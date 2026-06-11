using Myholas.Core.Dtos.DeserializationDtos.ESPDevices;
using Myholas.Core.Dtos.DeserializationDtos.Z2mDevices;
using Myholas.Core.Dtos.Devices;
using Myholas.Core.Interfaces;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace Myholas.BLL.Device
{
    // Синхронизатор устройств с БД
    // Слушает device.updated и config.received, обновляет данные устройств и сущностей в БД
    // EspDeviceDto / BaseEntityConfigDto в DeviceDto + EntityDto
    public class DeviceSynchronizerService
    {
        private readonly IEventBus _eventBus;
        private readonly IDeviceRepository _deviceRep;

        public DeviceSynchronizerService(IEventBus eventBus, IDeviceRepository deviceRep)
        {
            _eventBus = eventBus;
            _deviceRep = deviceRep;

            _eventBus.Listen("device.updated", OnDeviceUpdated);
            // Online/Offline
            _eventBus.Listen("device_status.updated", OnDeviceStatusUpdated);
            _eventBus.Listen("config.received", OnConfigReceived);
            CreateSystemTimeSenor();
        }

        // Обработчик конфигурации конкретной сущности
        private async void OnConfigReceived(string eventType, string data)
        {
            try
            {
                // Парсим deviceId + Config
                var wrapper = JsonSerializer.Deserialize<ConfigWrapper>(data);

                if (wrapper == null)
                    return;

                string deviceId = wrapper.DeviceId;
                EntityDto? entityDto = null;

                // ОПРЕДЕЛЯЕМ ТИП КОНФИГА (ESPHome или Z2M)
                using var doc = JsonDocument.Parse(wrapper.Config.GetRawText());
                var root = doc.RootElement;

                if (root.TryGetProperty("uniq_id", out _))
                {
                    // Это ESPHome
                    var config = JsonSerializer.Deserialize<BaseEntityConfigDto>(wrapper.Config.GetRawText());
                    entityDto = MapEspConfigToEntity(config);
                }
                else if (root.TryGetProperty("name", out _))
                {
                    // Это Zigbee2MQTT
                    var config = JsonSerializer.Deserialize<Z2MExposeDto>(wrapper.Config.GetRawText());
                    entityDto = MapZ2mExposeToEntity(config);
                }

                if (entityDto == null) return;

                // Минимальный DTO устройства для связи (согласно сигнатуре AddOrUpdateEntityAsync)
                var deviceDto = new DeviceDto { DeviceId = deviceId };

                // В БД
                await _deviceRep.AddOrUpdateEntityAsync(deviceDto, entityDto);
            }
            catch (Exception ex) { Console.WriteLine($"Error in OnConfigReceived: {ex.Message}"); }
        }


        // Обработчик обновления всего устройства  - из MQTT discovery
        private async void OnDeviceUpdated(string eventType, string data)
        {
            try
            {
                using var doc = JsonDocument.Parse(data);
                var root = doc.RootElement;
                DeviceDto? deviceDto = null;

                if (root.TryGetProperty("ieee_address", out _))
                {
                    // Обработка Z2M
                    var z2m = JsonSerializer.Deserialize<Z2MDeviceDto>(data);
                    if (z2m == null) return;
                    deviceDto = new DeviceDto
                    {
                        DeviceId = z2m.IeeeAddress,
                        FriendlyName = z2m.FriendlyName,
                        IsOnline = z2m.IsOnline,
                        LastSeen = DateTime.UtcNow
                    };
                }
                else if (root.TryGetProperty("name", out _))
                {
                    // Обработка ESPHome
                    var esp = JsonSerializer.Deserialize<EspDeviceDto>(data);
                    if (esp == null) return;
                    deviceDto = new DeviceDto
                    {
                        DeviceId = esp.Name,
                        IpAddress = esp.Ip,
                        Version = esp.Version,
                        IsOnline = esp.IsOnline,
                        LastSeen = DateTime.UtcNow,
                        FriendlyName = esp.FriendlyName
                    };

                    // Обработка сущностей, если они пришли в пакете устройства (специфика ESPHome)
                    if (esp.Entities != null && esp.Entities.Any())
                    {
                        foreach (var entityConfig in esp.Entities)
                        {
                            var entityDto = MapEspConfigToEntity(entityConfig);
                            await _deviceRep.AddOrUpdateEntityAsync(deviceDto, entityDto);
                        }
                    }
                }

                if (deviceDto == null) return;

                // Сохраняем заданное в БД имя, если оно есть
                var existDevice = await _deviceRep.GetByDeviceIdAsync(deviceDto.DeviceId);
                if (existDevice != null)
                {
                    deviceDto.FriendlyName = existDevice.FriendlyName;
                }

                // Обновляем устройство в БД (в репозитории должен быть метод UpdateDevice или аналогичный)
                // Так как в вашем IDeviceRepository нет UpdateDevice, используем UpdateDeviceStatusAsync или расширяем репозиторий
                await _deviceRep.UpdateDeviceStatusAsync(deviceDto);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[DeviceSynchronizer] Update error: {ex.Message}");
            }
        }

        // Вспомогательный метод для маппинга ESPHome
        private EntityDto MapEspConfigToEntity(BaseEntityConfigDto config)
        {
            return new EntityDto
            {
                EntityId = config.EntityId ?? $"{config.Domain}.{config.ObjectId}",
                Domain = config.Domain ?? "unknown",
                FriendlyName = config.Name ?? config.ObjectId,
                StateTopic = config.StateTopic,
                CommandTopic = config.CommandTopic,
                AttributesJson = SerializeAttributes(config)
            };
        }

        // Вспомогательный метод для маппинга Z2M
        private EntityDto MapZ2mExposeToEntity(Z2MExposeDto config)
        {
            return new EntityDto
            {
                EntityId = $"{GetZ2mDomain(config.Type)}.{config.Name}",
                Domain = GetZ2mDomain(config.Type),
                FriendlyName = config.Label ?? config.Name,
                AttributesJson = SerializeAttributes(config)
            };
        }

        private string GetZ2mDomain(string? type) => type switch
        {
            "numeric" => "sensor",
            "binary" => "binary_sensor",
            "enum" => "sensor",
            _ => "sensor"
        };

        private async void OnDeviceStatusUpdated(string eventType, string data)
        {
            try
            {
                using var doc = JsonDocument.Parse(data);
                var root = doc.RootElement;
                DeviceDto? deviceDto = null;

                if (root.TryGetProperty("ieee_address", out _))
                {
                    var z2m = JsonSerializer.Deserialize<Z2MDeviceDto>(data);
                    deviceDto = new DeviceDto { DeviceId = z2m?.IeeeAddress ?? "", IsOnline = z2m?.IsOnline ?? false };
                }
                else
                {
                    var esp = JsonSerializer.Deserialize<EspDeviceDto>(data);
                    deviceDto = new DeviceDto { DeviceId = esp?.Name ?? "", IsOnline = esp?.IsOnline ?? false };
                }

                if (deviceDto != null)
                {
                    await _deviceRep.UpdateDeviceStatusAsync(deviceDto);
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error: {ex.Message}");
            }
        }

        private async void CreateSystemTimeSenor()
        {
            var sensorDevice = new DeviceDto
            {
                DeviceId = "system",
                FriendlyName = "Система",
                Version = "1.0",
                IsOnline = true
            };

            var sensorEntity = new EntityDto
            {
                EntityId = "sensor.time",
                Domain = "sensor",
                FriendlyName = "Системное время"
            };

            await _deviceRep.AddOrUpdateEntityAsync(sensorDevice, sensorEntity);
        }

        // JSON атрибуты (универсальный метод)
        private string? SerializeAttributes(object config)
        {
            if (config == null) return null;

            var options = new JsonSerializerOptions
            {
                DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull,
                PropertyNamingPolicy = JsonNamingPolicy.CamelCase
            };

            if (config is SensorEntityConfigDto sensor)
            {
                return JsonSerializer.Serialize(new { sensor.UnitOfMeasurement, sensor.DeviceClass }, options);
            }
            if (config is SelectEntityConfigDto select)
            {
                return JsonSerializer.Serialize(new { select.Options }, options);
            }
            if (config is SwitchEntityConfigDto sw)
            {
                return JsonSerializer.Serialize(new { sw.PayloadOn, sw.PayloadOff }, options);
            }
            if (config is Z2MExposeDto z2m)
            {
                return JsonSerializer.Serialize(new { z2m.Type, z2m.Label }, options);
            }

            return null;
        }
    }

    // Контейнер для передачи DeviceId + Config
    public class ConfigWrapper
    {
        public string DeviceId { get; set; } = "";
        // Изменено на JsonElement, чтобы поддерживать любой тип конфига (ESP или Z2M)
        public JsonElement Config { get; set; }
    }
}
