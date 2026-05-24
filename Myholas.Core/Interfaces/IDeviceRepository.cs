using Myholas.Core.Dtos;

namespace Myholas.Core.Interfaces
{
    public interface IDeviceRepository
    {
        //Получить устройство по EntityId
        Task<DeviceEntityDto?> GetByIdAsync(string entityId);


        // Получить все устройства
        Task<List<DeviceEntityDto>> GetAllAsync(bool includeUnavailable = false);


        // Получить устройства по домену (switch, sensor, light, select)
        Task<List<DeviceEntityDto>> GetByDomainAsync(string domain);


        // Получить устройства по ID физического устройства
        Task<List<DeviceEntityDto>> GetByDeviceIdAsync(string deviceId);


        // Добавить новое устройство или обновить существующее
        Task<DeviceEntityDto> AddOrUpdateAsync(DeviceEntityDto entity);


        // Удалить устройство
        Task<bool> DeleteAsync(string entityId);


        // Проверить существование устройства
        Task<bool> ExistsAsync(string entityId);


        // Обновить состояние устройства и добавить запись в историю
        Task UpdateStateAsync(StateEntityDto stateEntity);


        // Получить историю состояний устройства
        Task<List<StateEntityDto>> GetStatesByEntityIdAsync(string entityId, int limit = 100);


        // Добавить только запись состояния (без обновления CurrentState)
        Task AddStateAsync(StateEntityDto stateEntity);


        // Обновить статус доступности устройства
        Task UpdateAvailabilityAsync(DeviceEntityDto deviceDto);
    }
}