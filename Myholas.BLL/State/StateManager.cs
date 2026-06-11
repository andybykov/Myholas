#define DEB
using AutoMapper;
using Myholas.Core.Dtos;
using Myholas.Core.Dtos.DeserializationDtos.ESPDevices;
using Myholas.Core.Dtos.Devices;
using Myholas.Core.Interfaces;
using Myholas.Core.Models.Output;
using System.Text.Json;

namespace Myholas.BLL.State
{
    // Управляет состояниями устройств, кэшированием и связью с БД
    public class StateManager : IStateManager
    {
        private readonly IEventBus _eventBus;
        private readonly IStateMachine _stateMachine; // Кэш: Key = string entityId, Value = StateEntityDto
        private readonly IStateRepository _stateRepo;
        private readonly IDeviceRepository _deviceRepo;
        private readonly IMapper _mapper;

        public StateManager(IEventBus eventBus, IStateMachine stateMachine, IStateRepository stateRepo, IDeviceRepository deviceRepo, IMapper mapper)
        {
            _eventBus = eventBus;
            _stateMachine = stateMachine;
            _stateRepo = stateRepo;
            _deviceRepo = deviceRepo;
            _mapper = mapper;

            // Подписка на изменение состояния
            _eventBus.Listen("state_changed", OnStateChanged);
        }

        // Обработчик изменения состояния (из MqttToEventBusBridge)
        private async void OnStateChanged(string eventType, string data)
        {
#if DEB
            Console.WriteLine($"[STATE MANAGER] OnStateChanged: {data}");
#endif
            try
            {
                // "deviceId|entityId:payload"
                var parts = data.Split('|');
                if (parts.Length < 2) 
                    return;

                string deviceId = parts[0];
                var stateParts = parts[1].Split(':');

                string entityId = stateParts[0];
                // Собираем payload 
                string rawPayload = string.Join(":", stateParts.Skip(1));

                string newState;
                string? attributesJson = null;

 
                if (entityId.StartsWith("light"))
                {
                    try
                    {
                        // Для света извлекаем только state
                        var lightStateDto = JsonSerializer.Deserialize<LightStateAttrDto>(rawPayload);
                        newState = lightStateDto?.State ?? "unknown";
                        attributesJson = rawPayload; // Весь JSON в атрибуты
                    }
                    catch
                    {
                        newState = "error";
                        attributesJson = rawPayload;
                    }
                }
                else
                {
                    // Для сенсоров и переключателей - СТРОКА
                    newState = rawPayload;
                }

                // Обновление кэша и БД
                var stateDto = _stateMachine.Get(entityId) ?? new StateEntityDto { EntityIdString = entityId };

                // если состояние не изменилось!
                if (stateDto.State == newState)
                    return;

                stateDto.State = newState;
                stateDto.CreatedAt = DateTime.UtcNow;
                stateDto.AttributesJson = attributesJson;

                await UpdateStateAsync(deviceId, stateDto);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[STATE MANAGER] Error: {ex.Message}");
            }
        }

        // Загрузка состояний из БД в StateMachine при старте
        public async Task InitializeCacheAsync()
        {
            // Получаем все устройства с их сущностями
            var allDevices = await _deviceRepo.GetAllDevicesAsync(includeUnavailable: true);

            foreach (var device in allDevices)
            {
                foreach (var entity in device.Entities)
                {
                    // Берем последнее состояние из истории
                    var lastState = await _stateRepo.GetLastStateAsync(entity.EntityId);

                    if (lastState != null)
                    {
                        _stateMachine.Set(lastState);
                    }
                    else if (!string.IsNullOrEmpty(entity.CurrentState))
                    {
                        // Создаем состояние из текущего значения сущности
                        var stubState = new StateEntityDto
                        {
                            EntityIdString = entity.EntityId,
                            State = entity.CurrentState,
                            AttributesJson = entity.AttributesJson,
                            CreatedAt = entity.UpdatedAt
                        };
                        _stateMachine.Set(stubState);
                    }
                }
            }
        }

        // Получить текущее состояние из КЭШ
        public async Task<EntityOutputModel?> GetCurrentStateAsync(string entityId)
        {
            var stateDto = _stateMachine.Get(entityId);
            if (stateDto == null) return null;

            // Получаем метаданные сущности из БД
            var entity = await _deviceRepo.GetByEntityIdAsync(entityId);
            if (entity == null) return null;

            var output = _mapper.Map<EntityOutputModel>(entity);
            output.State = stateDto.State;
            output.LastSeen = stateDto.CreatedAt;

            return output;
        }

        // Обновить состояние в кэше и БД
        public async Task UpdateStateAsync(string deviceId, StateEntityDto stateDto)
        {
            _stateMachine.Set(stateDto);
            // Сохраняем текущий статус и запись в историю
            await _stateRepo.UpdateStateAsync(deviceId, stateDto.EntityIdString, stateDto.State, stateDto.AttributesJson);
        }

        // Получить историю состояний сущности из БД
        public async Task<List<DeviceHistoryOutputModel>> GetHistoryAsync(string entityId, DateTime? from = null, DateTime? to = null, int limit = 100)
        {
            var states = await _stateRepo.GetHistoryAsync(entityId, from, to, limit);
            return _mapper.Map<List<DeviceHistoryOutputModel>>(states);
        }
    }
}
