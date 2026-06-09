using Myholas.Core.Dtos.Devices;
using Myholas.Core.Models.Input;
using Myholas.Core.Models.Output;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Myholas.Core.Interfaces
{
    public interface IDeviceManager
    {

        // Получить сущность EntityId     
        Task<EntityOutputModel?> GetEntityByIdAsync(string entityId);


        // Получить список сущностей
        Task<List<EntityOutputModel>> GetAllEntitiesAsync(bool includeUnavailable = false);


        // Добавить/обновить  устройство + сущность 
        Task<EntityDto> AddOrUpdateAsync(DeviceDtoInputModel device, EntityDtoInputModel entity);


        // Получить сгруппированный список устройств 
        Task<List<DeviceOutputModel>> GetGroupedDevicesAsync();

        // Получить сущности по домену
        Task<List<EntityOutputModel>> GetEntitiesByDomainAsync(string domain);


        // Удалить 
        Task<bool> DeleteAsync(string entityId);


        // Проверить   
        Task<bool> ExistsAsync(string entityId);


        // Получить все сущности принадлежащие физическому устройству 
        Task<List<EntityOutputModel>> GetEntitiesByDeviceIdAsync(string deviceId);
    }
}
