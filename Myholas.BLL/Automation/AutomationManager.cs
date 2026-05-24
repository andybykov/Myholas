using Myholas.Core.Automation;
using Myholas.Core.Dtos;
using Myholas.Core.Interfaces;

namespace Myholas.BLL.Automation
{
    /// <summary>
    /// Manager работы с automation 
    /// </summary>
    public class AutomationManager : IAutomationManager
    {
        private readonly IAutomationRepository _repository;


        public AutomationManager(
            IAutomationRepository repository)
        {
            _repository = repository;
        }



        // Получить по ID
        public async Task<AutomationEntityDto?> GetByIdAsync(int id)
        {
            return await _repository.GetByIdAsync(id);
        }


        // Получить все 
        public async Task<List<AutomationEntityDto>> GetAllAsync(bool includeDisabled = false)
        {
            return await _repository.GetAllAsync(
                includeDisabled);
        }


        // Получить только enabled automation
        public async Task<List<AutomationEntityDto>> GetEnabledAsync()
        {
            return await _repository.GetEnabledAsync();
        }


        // Создание automation
        public async Task<AutomationEntityDto> AddAsync(AutomationEntityDto dto)
        {
            ValidateAutomation(dto);

            dto.CreatedAt = DateTime.UtcNow;

            var created =
                await _repository.AddAsync(dto);

            return created;
        }


        // Обновление automation
        public async Task<AutomationEntityDto?> UpdateAsync(int id, AutomationEntityDto dto)
        {
            var existing =
                await _repository.GetByIdAsync(id);

            if (existing == null)
                return null;

            ValidateAutomation(dto);

            dto.Id = id;

            dto.CreatedAt = existing.CreatedAt;

            dto.UpdatedAt = DateTime.UtcNow;

            var updated = await _repository.UpdateAsync(dto);


            return updated;
        }



        // Удаление automation

        public async Task<bool> DeleteAsync(int id)
        {
            var deleted =
                await _repository.DeleteAsync(id);

            if (!deleted)
                return false;

            return true;
        }


        // Включение / выключение automation

        public async Task<bool> SetEnabledAsync(
            int id,
            bool enabled)
        {
            var result =
                await _repository.SetEnabledAsync(
                    id,
                    enabled);

            if (!result)
                return false;

            return true;
        }



        // Валидация automation

        private void ValidateAutomation(AutomationEntityDto dto)
        {
            if (string.IsNullOrWhiteSpace(dto.Name))
            {
                throw new Exception(
                    "Automation name required");
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
                if (string.IsNullOrWhiteSpace(
                    trigger.EntityId))
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
                    if (string.IsNullOrWhiteSpace(
                        action.EntityId))
                    {
                        throw new Exception(
                            "Action entity required");
                    }

                    if (string.IsNullOrWhiteSpace(
                        action.Command))
                    {
                        throw new Exception(
                            "Action command required");
                    }
                }
            }
        }

    }
}
