using AutoMapper;
using Myholas.Core.Automation;
using Myholas.Core.Dtos.Automations;
using Myholas.Core.Interfaces;
using Myholas.Core.Models.Input;
using Myholas.Core.Models.Output;

namespace Myholas.BLL.Automation
{
    // Manager работы с automation 
    public class AutomationManager : IAutomationManager
    {
        private readonly IAutomationRepository _autoRepo;
        private readonly IDeviceRepository _deviceRepo;
        private readonly IMapper _mapper;
        private readonly IEventBus _eventBus;

        public AutomationManager(IAutomationRepository autoRepo, IDeviceRepository deviceRepo, IMapper mapper, IEventBus eventBus)
        {
            _autoRepo = autoRepo;
            _deviceRepo = deviceRepo;
            _mapper = mapper;
            _eventBus = eventBus;
        }

        // Получить по ID
        public async Task<AutomationOutputModel?> GetByIdAsync(int id)
        {
            var automation = await _autoRepo.GetByIdAsync(id);

            if (automation == null)
                return null;

            return _mapper.Map<AutomationOutputModel>(automation);
        }

        // Получить все 
        public async Task<List<AutomationOutputModel>> GetAllAsync(bool includeDisabled = false)
        {
            var automations = await _autoRepo.GetAllAsync(includeDisabled);
            return _mapper.Map<List<AutomationOutputModel>>(automations);
        }

        // Получить только enabled automation
        public async Task<List<AutomationOutputModel>> GetEnabledAsync()
        {
            var automations = await _autoRepo.GetEnabledAsync();
            return _mapper.Map<List<AutomationOutputModel>>(automations);
        }

        // Создание automation
        public async Task<AutomationOutputModel> AddAsync(AutomationInputModel input)
        {
            // Маппим в DTO для валидации
            var dto = _mapper.Map<AutomationEntityDto>(input);
            ValidateAutomation(dto);

            //  ID сущности по строковому имени
            var entity = await _deviceRepo.GetByEntityIdAsync(input.EntityId);
            if (entity == null)
                throw new Exception($"Entity {input.EntityId} not found!");

            // Привязываем  ID и дату
            dto.EntityId = entity.Id;
            dto.CreatedAt = DateTime.UtcNow;

            var created = await _autoRepo.AddAsync(dto);

            _eventBus.Emit("automation.created", created.Id.ToString());

            return _mapper.Map<AutomationOutputModel>(created);
        }

        // Обновление automation
        public async Task<AutomationOutputModel?> UpdateAsync(int id, AutomationInputModel input)
        {
            var existing = await _autoRepo.GetByIdAsync(id);
            if (existing == null) return null;

            // Находим ID сущности для обновления
            var entity = await _deviceRepo.GetByEntityIdAsync(input.EntityId);
            if (entity == null) throw new Exception("Entity not found");

            var dto = _mapper.Map<AutomationEntityDto>(input);
            ValidateAutomation(dto);

            dto.Id = id;
            dto.EntityId = entity.Id; // Привязываем к правильному ID
            dto.CreatedAt = existing.CreatedAt;
            dto.UpdatedAt = DateTime.UtcNow;

            var updated = await _autoRepo.UpdateAsync(dto);

            _eventBus.Emit("automation.updated", updated.Id.ToString());

            return _mapper.Map<AutomationOutputModel>(updated);
        }

        // Удаление automation
        public async Task<bool> DeleteAsync(int id)
        {
            _eventBus.Emit("automation.deleted", id.ToString());

            return await _autoRepo.DeleteAsync(id);
        }

        // Включение / выключение automation
        public async Task<bool> SetEnabledAsync(int id, bool enabled)
        {
            return await _autoRepo.SetEnabledAsync(id, enabled);
        }

        // Валидация automation
        private void ValidateAutomation(AutomationEntityDto dto)
        {
            if (string.IsNullOrWhiteSpace(dto.Name))
            {
                throw new Exception("Automation name required");
            }

            // triggers required
            var triggers = dto.GetTriggers();
            if (!triggers.Any())
            {
                throw new Exception("At least one trigger required");
            }

            // actions required
            var actions = dto.GetActions();
            if (!actions.Any())
            {
                throw new Exception("At least one action required");
            }

            // validate triggers
            foreach (var trigger in triggers)
            {
                if (string.IsNullOrWhiteSpace(trigger.EntityId))
                {
                    throw new Exception("Trigger entity required");
                }

                if (string.IsNullOrWhiteSpace(trigger.Operator))
                {
                    throw new Exception("Trigger operator required");
                }
            }

            // validate actions
            foreach (var action in actions)
            {
                if (action.Type == "command")
                {
                    if (string.IsNullOrWhiteSpace(action.EntityId))
                    {
                        throw new Exception("Action entity required");
                    }

                    if (string.IsNullOrWhiteSpace(action.Command))
                    {
                        throw new Exception("Action command required");
                    }
                }
            }
        }
    }
}
