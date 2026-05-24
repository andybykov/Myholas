using Myholas.Core.Dtos;

namespace Myholas.Core.Interfaces
{

    public interface IAutomationManager
    {

        Task<AutomationEntityDto?> GetByIdAsync(int id);

        Task<List<AutomationEntityDto>> GetAllAsync(bool includeDisabled = false);

        Task<List<AutomationEntityDto>> GetEnabledAsync();

        Task<AutomationEntityDto> AddAsync(AutomationEntityDto dto);

        Task<AutomationEntityDto?> UpdateAsync(int id, AutomationEntityDto dto);

        Task<bool> DeleteAsync(int id);

        Task<bool> SetEnabledAsync(int id, bool enabled);
    }
}
