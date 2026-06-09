using Myholas.Core.Dtos.Devices;
using Myholas.Core.Dtos.ESPDevices;
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
                // Парсим eviceId + Config
                var wrapper = JsonSerializer.Deserialize<ConfigWrapper>(data);

                if (wrapper == null || wrapper.Config == null)
                    return;

                var config = wrapper.Config;
                string deviceId = wrapper.DeviceId;

                // Минимальный DTO устройства для связи
                var deviceDto = new DeviceDto { DeviceId = deviceId };

                // Данные сущности из конфига
                var entityDto = new EntityDto
                {
                    EntityId = config.EntityId ?? $"{config.Domain}.{config.ObjectId}",
                    Domain = config.Domain ?? "unknown",
                    FriendlyName = config.Name ?? config.ObjectId,
                    StateTopic = config.StateTopic,
                    CommandTopic = config.CommandTopic,
                    AttributesJson = SerializeAttributes(config)
                };

                // Спец. поля для сенсоров
                if (config is SensorEntityConfigDto sensor)
                {
                    entityDto.UnitOfMeasurement = sensor.UnitOfMeasurement;
                }

                // В БД
                await _deviceRep.AddOrUpdateEntityAsync(deviceDto, entityDto);
            }
            catch (Exception ex) { Console.WriteLine($"Error: {ex.Message}"); }
        }


        // Обработчик обновления всего устройства  - из MQTT discovery
        private async void OnDeviceUpdated(string eventType, string data)
        {
            try
            {
                var espDevice = JsonSerializer.Deserialize<EspDeviceDto>(data);
                if (espDevice == null || string.IsNullOrEmpty(espDevice.Name))
                    return;

                var existDevice = await _deviceRep.GetByDeviceIdAsync(espDevice.Name);

                // Dto для обновления
                var deviceDto = new DeviceDto
                {
                    DeviceId = espDevice.Name,
                    IpAddress = espDevice.Ip,
                    Version = espDevice.Version,
                    IsOnline = espDevice.IsOnline,
                    LastSeen = DateTime.UtcNow
                };

               if (existDevice == null)
                {
                    //  имя из MQTT
                    deviceDto.FriendlyName = espDevice.FriendlyName;
                }
                else
                {
                    // заданное ИМЯ
                    deviceDto.FriendlyName = existDevice.FriendlyName;
                }

                // Обработка сущностей
                if (espDevice.Entities != null && espDevice.Entities.Any())
                {
                    foreach (var entityConfig in espDevice.Entities)
                    {
                        var entityDto = MapToEntityDto(entityConfig);
                        await _deviceRep.AddOrUpdateEntityAsync(deviceDto, entityDto);
                    }
                }
                else
                {
                    var emptyEntity = new EntityDto { EntityId = "system.device_info" };
                    await _deviceRep.AddOrUpdateEntityAsync(deviceDto, emptyEntity);
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[DeviceSynchronizer] Update error: {ex.Message}");
            }
        }


        // Вспомогательный метод для маппинга сущностей
        private EntityDto MapToEntityDto(BaseEntityConfigDto config)
        {
            var entityDto = new EntityDto
            {
                EntityId = $"{config.Domain}.{config.ObjectId}",
                Domain = config.Domain ?? "unknown",
                FriendlyName = config.Name ?? config.ObjectId,
                StateTopic = config.StateTopic,
                CommandTopic = config.CommandTopic,
                AttributesJson = SerializeAttributes(config)
            };

            // Специфичные поля для сенсоров
            if (config is SensorEntityConfigDto sensor)
            {
                entityDto.UnitOfMeasurement = sensor.UnitOfMeasurement;
            }

            return entityDto;
        }



        private async void OnDeviceStatusUpdated(string eventType, string data)
        {
            try
            {
                var espDevice = JsonSerializer.Deserialize<EspDeviceDto>(data);
                if (espDevice == null)
                    return;

                // копируем данные
                var deviceDto = new DeviceDto
                {
                    DeviceId = espDevice.Name,
                    IsOnline = espDevice.IsOnline
                };

                await _deviceRep.UpdateDeviceStatusAsync(deviceDto);
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


        // JSON атрибуты 
        private string? SerializeAttributes(BaseEntityConfigDto config)
        {
            if (config == null)
                return null;

            var options = new JsonSerializerOptions
            {
                DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull,
                PropertyNamingPolicy = JsonNamingPolicy.CamelCase
            };

            //  поля в зависимости от типа
            if (config is SelectEntityConfigDto select)
            {
                var subset = new { select.Options };
                return JsonSerializer.Serialize(subset, options);
            }

            if (config is SensorEntityConfigDto sensor)
            {
                var subset = new { sensor.UnitOfMeasurement, sensor.DeviceClass };
                return JsonSerializer.Serialize(subset, options);
            }

            if (config is SwitchEntityConfigDto sw)
            {
                var subset = new { sw.PayloadOn, sw.PayloadOff };
                return JsonSerializer.Serialize(subset, options);
            }

            return null;
        }
    }

    
    // Контейнер для передачи DeviceId + Config
    public class ConfigWrapper
    {
        public string DeviceId { get; set; }
        public BaseEntityConfigDto Config { get; set; }
    }
}
