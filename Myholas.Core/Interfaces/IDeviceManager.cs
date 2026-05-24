using Myholas.Core.Dtos;
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
        Task<List<DeviceOutputModels>> GetGroupedDevicesAsync();

        Task<List<EntityOutputModel>> GetAllEntitiesAsync(bool includeUnavailable = false);

        Task<EntityOutputModel?> GetEntityByIdAsync(string entityId);

        Task<List<EntityOutputModel>> GetEntitiesByDomainAsync(string domain);

        Task<List<EntityOutputModel>> GetEntitiesByDeviceIdAsync(string deviceId);

        Task<DeviceEntityDto> AddOrUpdateAsync(DeviceEntityDto entity);

        Task<bool> DeleteAsync(string entityId);

        Task<bool> ExistsAsync(string entityId);
    }
}
