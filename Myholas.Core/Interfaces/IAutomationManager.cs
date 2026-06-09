using Myholas.Core.Models.Input;
using Myholas.Core.Models.Output;

namespace Myholas.Core.Interfaces
{
    // Объект‑менеджер для работы с автоматизациями
    public interface IAutomationManager
    {
        // Получить автоматизацию по Id
        Task<AutomationOutputModel?> GetByIdAsync(int id);


        // Получить список всех автоматизаций 
        Task<List<AutomationOutputModel>> GetAllAsync(bool includeDisabled = false);


        // Получить тольков вкл автоматизации
        Task<List<AutomationOutputModel>> GetEnabledAsync();


        // Создать новую автоматизацию
        Task<AutomationOutputModel> AddAsync(AutomationInputModel input);


        // Обновить существующую автоматизацию
        Task<AutomationOutputModel?> UpdateAsync(int id, AutomationInputModel input);


        // Удалить автоматизацию
        Task<bool> DeleteAsync(int id);


        // Включить/выключить автоматизацию
        Task<bool> SetEnabledAsync(int id, bool enabled);
    }
}
