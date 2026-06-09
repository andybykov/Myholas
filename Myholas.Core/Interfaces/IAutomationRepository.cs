using Myholas.Core.Dtos.Automations;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Myholas.Core.Interfaces
{
    public interface IAutomationRepository
    {
        Task<AutomationEntityDto?> GetByIdAsync(int id);

        Task<List<AutomationEntityDto>> GetAllAsync(bool includeDisabled = false);

        Task<List<AutomationEntityDto>> GetEnabledAsync();

        Task<AutomationEntityDto> AddAsync(AutomationEntityDto automation);

        Task<AutomationEntityDto> UpdateAsync(AutomationEntityDto automation);

        Task<bool> DeleteAsync(int id);

        Task<bool> SetEnabledAsync(int id, bool isEnabled);
    }
}
