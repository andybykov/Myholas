using Myholas.Core.Dtos.Devices;

namespace Myholas.Core.Interfaces
{
    public interface IStateRepository
    {
        /// <summary>
        /// Обновляет текущий статус сущности и сохраняет запись в историю.
        /// При отсутствии сущности создаёт «заглушку» (stub) для поддержания FK‑связей.
        /// </summary>
        Task UpdateStateAsync(string deviceId, string entityId, string state, string? attributesJson = null);

        /// <summary>
        /// Возвращает историю состояний указанной сущности.
        /// Параметры <c>from</c> / <c>to</c> позволяют ограничить диапазон времени,
        /// <c>limit</c> задаёт максимальное количество записей (по умолчанию 100).
        /// </summary>
        Task<List<StateEntityDto>> GetHistoryAsync(
            string entityId,
            DateTime? from = null,
            DateTime? to = null,
            int limit = 100);

        /// <summary>
        /// Получает последнюю (самую свежую) запись состояния для указанной сущности.
        /// </summary>
        Task<StateEntityDto?> GetLastStateAsync(string entityId);

        /// <summary>
        /// Удаляет из истории все записи, старше указанной даты.
        /// </summary>
        Task DeleteOldAsync(DateTime olderThan);
        //Task UpdateStateAsync(int entityId, string? state, string? attributesJson);
    }
}
