#define DEB
using AutoMapper;
using Myholas.Core.Dtos;
using Myholas.Core.Dtos.ESPDevices;
using Myholas.Core.Interfaces;
using Myholas.Core.Models.Output;
using System.Text.Json;

namespace Myholas.BLL.States
{

    // Управляет состояниями устройств и связью их с БД    
    public class StateManager : IStateManager
    {
        private readonly IEventBus _eventBus;

        private readonly IStateMachine _stateMachine;

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

            //  из MqttToEventBusBridge
            _eventBus.Listen("state_changed", OnStateChanged);
           // _eventBus.Listen("command.recived", OnCommandReceived);
        }

        // Обработчик изменения состояния (из MqttToEventBusBridge)   
        private async void OnStateChanged(string eventType, string data)
        {
#if DEB
            Console.WriteLine($"[STATE MANAGER] OnStateChanged: {data}");
#endif

            string? entityId;
            string? newState;

            var light = data.StartsWith("light");
            if (!light)
            {
                // "switch.lamp01:on"
                var parts = data.Split(':');
                
                entityId = parts[0];
                newState = parts[1];

                // Получаем последний DTO из кэша или создаем 
                var stateDto = _stateMachine.Get(entityId) ?? new StateEntityDto { EntityId = entityId };             

                
                stateDto.State = newState;
                stateDto.CreatedAt = DateTime.UtcNow;

                await UpdateStateAsync(stateDto);
            }
            else
            {
                var parts = data.Split(':');
                entityId = parts[0];
                string json = string.Join(":", parts.Skip(1));
                var lightStateDto = JsonSerializer.Deserialize<LightStateAttrDto>(json);
                var stateDto = _stateMachine.Get(entityId) ?? new StateEntityDto { EntityId = entityId };

                newState = lightStateDto.State;

                stateDto.State = newState;
                stateDto.CreatedAt = DateTime.UtcNow;

                await UpdateStateAsync(stateDto);
            }           
        }
               
   

        // Загружаем все текущие состояния из БД в StateMachine 
        // Вызывается один раз в MqttBackgroundService после подключения к MQTT
        public async Task InitializeCacheAsync()
        {
            var allDevices = await _deviceRepo.GetAllAsync(includeUnavailable: true);
            foreach (var device in allDevices)
            {
                // Пытаемся взять последнее состояние из истории
                var lastState = await _stateRepo.GetLastStateAsync(device.EntityId);
                if (lastState != null)
                {
                    _stateMachine.Set(lastState);
                }
                else if (!string.IsNullOrEmpty(device.CurrentState))
                {
                    // StateEntityDto из CurrentState
                    var stubState = new StateEntityDto
                    {
                        EntityId = device.EntityId,
                        State = device.CurrentState,
                        AttributesJson = device.AttributesJson,
                        CreatedAt = device.UpdatedAt ?? DateTime.UtcNow
                    };
                    _stateMachine.Set(stubState);
                }
            }
        }


        // Получить текущее состояние из КЭШ
        public async Task<EntityOutputModel?> GetCurrentStateAsync(string entityId)
        {
            var stateDto = _stateMachine.Get(entityId);
            if (stateDto == null) 

                return null;

            var device = await _deviceRepo.GetByIdAsync(entityId);
            if (device == null)

                return null;

            var entity = _mapper.Map<EntityOutputModel>(device);
            entity.State = stateDto.State;
            entity.LastSeen = stateDto.CreatedAt;

            return entity;
        }

        // Обновить состояние устройства 
        public async Task UpdateStateAsync(StateEntityDto stateDto)
        {
            // в кэш
            _stateMachine.Set(stateDto);

            // Для истории НОВЫЙ объект
            var historyEntry = new StateEntityDto
            {
                EntityId = stateDto.EntityId,
                State = stateDto.State,
                AttributesJson = stateDto.AttributesJson,
                CreatedAt = DateTime.UtcNow
            };
            // в БД 
            await _stateRepo.AddStateAsync(historyEntry);

            // Обновляем DeviceEntityDto.CurrentState
            await _deviceRepo.UpdateStateAsync(stateDto);
        }

        // Получить историю состояний устройства из БД
        public async Task<List<DeviceHistoryOutputModel>> GetHistoryAsync(string entityId, DateTime? from = null, DateTime? to = null, int limit = 100)
        {
            var states = await _stateRepo.GetHistoryAsync(entityId, from, to, limit);

            return _mapper.Map<List<DeviceHistoryOutputModel>>(states);
        }
    }
}
