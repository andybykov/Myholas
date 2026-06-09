using Myholas.Core.Dtos.Devices;

namespace Myholas.Core.Interfaces
{
    public interface IStateRepository
    {

        // Обновляет текущий статус сущности и сохраняет запись в историю 
        Task UpdateStateAsync(string deviceId, string entityId, string state, string? attributesJson = null);


        // Возвращает историю состояний указанной сущности
        Task<List<StateEntityDto>> GetHistoryAsync(
            string entityId,
            DateTime? from = null,
            DateTime? to = null,
            int limit = 100);


        // Получает последнюю запись состояния
        Task<StateEntityDto?> GetLastStateAsync(string entityId);


        // Удаляет из истории все записи старше указанной даты
        Task DeleteOldAsync(DateTime olderThan);

        //Task UpdateStateAsync(int entityId, string? state, string? attributesJson);
    }
}
