using Myholas.Core.Dtos;
using Myholas.Core.Models.Output;

namespace Myholas.Core.Interfaces
{
    public interface IStateManager
    {
        // Получить текущее состояние 
        Task<EntityOutputModel?> GetCurrentStateAsync(string entityId);        


        // Получить историю состояний из БД
        Task<List<DeviceHistoryOutputModel>> GetHistoryAsync(string entityId, DateTime? from = null, DateTime? to = null, int limit = 100);

        // Загрузить все текущие состояния в кэш 
        Task InitializeCacheAsync();

        // Обновить состояние
        Task UpdateStateAsync(StateEntityDto stateDto);   
    }

}