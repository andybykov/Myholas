using Myholas.Core.Dtos;

namespace Myholas.Core.Interfaces
{
    public interface IStateRepository
    {
        Task<StateEntityDto> AddStateAsync(StateEntityDto state);

        Task<List<StateEntityDto>> GetHistoryAsync(string entityId, DateTime? from = null, DateTime? to = null, int limit = 100);

        Task<StateEntityDto?> GetLastStateAsync(string entityId);

        Task DeleteOldAsync(DateTime olderThan);
    }
}
