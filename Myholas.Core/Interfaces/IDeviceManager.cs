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
        /// <summary>
        /// Получить одну сущность по её строковому идентификатору (EntityId).
        /// </summary>
        Task<EntityOutputModel?> GetEntityByIdAsync(string entityId);

        /// <summary>
        /// Получить список всех сущностей всех устройств.
        /// Параметр <c>includeUnavailable</c> включает/исключает офлайн‑устройства.
        /// </summary>
        Task<List<EntityOutputModel>> GetAllEntitiesAsync(bool includeUnavailable = false);

        /// <summary>
        /// Добавить новое устройство + сущность либо обновить их, если уже существуют.
        /// Возвращает сохранённый <c>EntityDto</c>.
        /// </summary>
        Task<EntityDto> AddOrUpdateAsync(DeviceDtoInputModel device, EntityDtoInputModel entity);

        /// <summary>
        /// Получить сгруппированный список устройств со вложенными сущностями.
        /// </summary>
        Task<List<DeviceOutputModel>> GetGroupedDevicesAsync();

        // Получить сущности по домену
        Task<List<EntityOutputModel>> GetEntitiesByDomainAsync(string domain);

        /// <summary>
        /// Удалить устройство/сущность по её EntityId.
        /// </summary>
        Task<bool> DeleteAsync(string entityId);

        /// <summary>
        /// Проверить, существует ли запись с указанным EntityId.
        /// </summary>
        Task<bool> ExistsAsync(string entityId);

        /// <summary>
        /// Получить все сущности, принадлежащие физическому устройству (по DeviceId).
        /// </summary>
        Task<List<EntityOutputModel>> GetEntitiesByDeviceIdAsync(string deviceId);
    }
}
