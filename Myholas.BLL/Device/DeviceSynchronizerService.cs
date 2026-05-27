using Myholas.Core.Dtos;
using Myholas.Core.Dtos.ESPDevices;
using Myholas.Core.Interfaces;
using Myholas.Core.Models.Output;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace Myholas.BLL.Device
{
    // Синхронизатор устройств с БД
    // Слушает device.updated и сохраняет конфигурации в БД
    // EspDeviceDto в DeviceEntityDto 
    public class DeviceSynchronizerService
    {
        private readonly IEventBus _eventBus;

        private readonly IDeviceManager _deviceManager;

        public DeviceSynchronizerService(IEventBus eventBus, IDeviceManager deviceManager)
        {
            _eventBus = eventBus;
            _deviceManager = deviceManager;

            // event on MqttToEventBusBridge
            _eventBus.Listen("device.updated", OnDeviceUpdated);
            //  _eventBus.Listen("device.updated", OnConfigReceived);
        }

        // Обработчик конфигурации 
        private async void OnConfigReceived(string eventType, string data)
        {
            try
            {
                var config = JsonSerializer.Deserialize<BaseEntityConfigDto>(data);
                if (config == null)
                    return;

                string? entityId = config.EntityId;

                EntityOutputModel? model = await _deviceManager.GetEntityByIdAsync(entityId);

                if (model != null)
                {
                    switch (config)
                    {
                        case LightEntityConfigDto ld:
                            break;

                        case SensorEntityConfigDto sd:
                            model.UnitOfMeasurement = sd.UnitOfMeasurement;
                            break;
                        case SelectEntityConfigDto sl:
                            model.Options = sl.Options.ToList() ?? null;
                            break;
                        default:
                            break;


                    }
                    // Пока просто лог
                    Console.WriteLine($"[DeviceSynchronizer] Config received for {entityId}");
                }

            }
            catch (Exception ex)
            {
                Console.WriteLine($"[DeviceSynchronizer] Config error: {ex.Message}");
            }
        }

        // Обработчик обновления устройства из MQTT discovery
        private async void OnDeviceUpdated(string eventType, string data)
        {
            try
            {
                // data — сериализованный EspDeviceDto
                EspDeviceDto? espDevice = JsonSerializer.Deserialize<EspDeviceDto>(data);
                if (espDevice == null)
                    return;

                // Преобразуем EspDeviceDto в DeviceEntityDto 
                // для каждой entity внутри espDevice.Entities создать DeviceEntityDto
                foreach (var entityConfig in espDevice.Entities)
                {
                    var deviceEntity = new DeviceEntityDto
                    {
                        EntityId = $"{entityConfig.Domain}.{entityConfig.ObjectId}",
                        DeviceId = espDevice.Name,
                        Domain = entityConfig.Domain,
                        FriendlyName = entityConfig.Name ?? entityConfig.ObjectId,
                        IpAdress = espDevice.Ip,
                        CommandTopic = entityConfig.CommandTopic,
                        StateTopic = entityConfig.StateTopic,
                        //CurrentState 
                        UnitOfMeasurement = GetUnitOfMeasurement(entityConfig),
                        AttributesJson = SerializeAttributes(entityConfig),
                        IsAvailable = true,
                        LastSeen = DateTime.UtcNow,
                        CreatedAt = DateTime.UtcNow,
                        UpdatedAt = DateTime.UtcNow
                    };

                    await _deviceManager.AddOrUpdateAsync(deviceEntity);
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[DeviceSynchronizer] Error: {ex.Message}");
            }
        }


        // Заполнение поля Attributes
        private string? SerializeAttributes(BaseEntityConfigDto config)
        {
            if (config == null)
                return null;

            var options = new JsonSerializerOptions
            {
                DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull,
                PropertyNamingPolicy = JsonNamingPolicy.CamelCase
            };

            // select: сериализуем весь объект
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

        // GetUnitOfMeasurement
        private string? GetUnitOfMeasurement(BaseEntityConfigDto config)
        {
            if (config == null)
                return null;

            if (config is SensorEntityConfigDto sensor)
            {
                var s = sensor.UnitOfMeasurement;

                return sensor.UnitOfMeasurement.ToString() ?? null;
            }

            return null;
        }
    }
}
