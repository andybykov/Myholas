using Myholas.Core.Dtos.Devices;

namespace Myholas.Core.Interfaces
{
    public interface IDeviceRepository
    {
        /// <summary>
        /// Получить объект EntityDto по его уникальному идентификатору (EntityId).
        /// </summary>
        Task<EntityDto?> GetByEntityIdAsync(string entityId);

        /// <summary>
        /// Получить список всех физических устройств с их датчиками.
        /// При <c>includeUnavailable = false</c> возвращаются только онлайн‑устройства.
        /// </summary>
        Task<List<DeviceDto>> GetAllDevicesAsync(bool includeUnavailable = false);

        /// <summary>
        /// Получить конкретное физическое устройство по его DeviceId (например, «esp‑lamp01»).
        /// </summary>
        Task<DeviceDto?> GetByDeviceIdAsync(string deviceId);


        // Получить все сущности по домену (switch, sensor, light, select)
        Task<List<EntityDto>> GetByDomainAsync(string domain);

        /// <summary>
        /// Создать новое устройство/датчик или обновить существующее.
        /// Возвращает сохранённый <c>EntityDto</c>.
        /// </summary>
        Task<EntityDto> AddOrUpdateEntityAsync(DeviceDto deviceInfo, EntityDto entityInfo);

        /// <summary>
        /// Удалить сущность (датчик) по её EntityId.
        /// Возвращает <c>true</c>, если удаление прошло успешно.
        /// </c>true</c> в противном случае.
        /// </summary>
        Task<bool> DeleteEntityAsync(string entityId);

        /// <summary>
        /// Проверить, существует ли сущность с заданным EntityId.
        /// </summary>
        Task<bool> ExistsAsync(string entityId);
    }
}