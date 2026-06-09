using Myholas.Core.Dtos.Devices;

namespace Myholas.Core.Interfaces
{
    public interface IDeviceRepository
    {

        // Получить сущность по EntityId
        Task<EntityDto?> GetByEntityIdAsync(string entityId);

  
        // Получить список всех физических устройств с их датчиками  
        Task<List<DeviceDto>> GetAllDevicesAsync(bool includeUnavailable = false);


        // Получить  физическое устройство по DeviceId
        Task<DeviceDto?> GetByDeviceIdAsync(string deviceId);


        // Получить все сущности по домену 
        Task<List<EntityDto>> GetByDomainAsync(string domain);


        // Создать/ обновить устройство
        Task<EntityDto> AddOrUpdateEntityAsync(DeviceDto deviceInfo, EntityDto entityInfo);


        // Обновить online/offline
        Task<bool> UpdateDeviceStatusAsync(DeviceDto deviceDto);

 
        // Удалить сущность по EntityId       
        Task<bool> DeleteEntityAsync(string entityId);


        // существует ли сущность EntityId
        Task<bool> ExistsAsync(string entityId);
    }
}