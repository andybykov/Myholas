using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Myholas.Core.Interfaces;
using Myholas.Core.Models.Input;
using Myholas.Core.Models.Output;

namespace Myholas.API.Controllers
{
    [Authorize]
    [ApiController]
    [Route("api/[controller]")]
    public class DevicesController : ControllerBase
    {
        private readonly IDeviceManager _deviceManager;

        public DevicesController(IDeviceManager deviceManager)
        {
            _deviceManager = deviceManager;
        }

        // Получить все устройства в виде списка
        [HttpGet("entities")]
        public async Task<ActionResult<List<EntityOutputModel>>> GetAllEntities([FromQuery] bool includeUnavailable = false)
        {
            var entities = await _deviceManager.GetAllEntitiesAsync(includeUnavailable);

            return Ok(entities);
        }

        // Получить устройство по EntityId 
        [HttpGet("entities/{entityId}")]
        public async Task<ActionResult<EntityOutputModel>> GetEntityById(string entityId)
        {
            var entity = await _deviceManager.GetEntityByIdAsync(entityId);
            if (entity == null)
                return NotFound($"Entity {entityId} not found");

            return Ok(entity);
        }

        // Получить устройства по домену
        [HttpGet("entities/by-domain/{domain}")]
        public async Task<ActionResult<List<EntityOutputModel>>> GetEntitiesByDomain(string domain)
        {
            var entities = await _deviceManager.GetEntitiesByDomainAsync(domain);
            return Ok(entities);
        }

        // Получить устройства по DeviceId 
        [HttpGet("entities/by-device/{deviceId}")]
        public async Task<ActionResult<List<EntityOutputModel>>> GetEntitiesByDeviceId(string deviceId)
        {
            var entities = await _deviceManager.GetEntitiesByDeviceIdAsync(deviceId);

            return Ok(entities);
        }

        // Получить группированные устройства 
        [HttpGet("groups")]
        public async Task<ActionResult<List<DeviceOutputModel>>> GetGroupedDevices()
        {
            var groups = await _deviceManager.GetGroupedDevicesAsync();

            return Ok(groups);
        }

        // CRUD 

        // Добавить или обновить устройство и его сущность
        // Используем специальный Request DTO, так как теперь нам нужны данные и для Device, и для Entity
        [Authorize(Roles = "Admin")]
        [HttpPost]
        public async Task<ActionResult<EntityDtoInputModel>> AddOrUpdate([FromBody] DeviceEntityRequest request)
        {
            if (request.Entity == null || string.IsNullOrWhiteSpace(request.Entity.EntityId))
                return BadRequest("Entity information and EntityId are required");

            if (request.Device == null || string.IsNullOrWhiteSpace(request.Device.DeviceId))
                return BadRequest("Device information and DeviceId are required");


            var dbEntity = await _deviceManager.AddOrUpdateAsync(request.Device, request.Entity);


            var resultDto = new EntityDtoInputModel
            {
                EntityId = dbEntity.EntityId,
                FriendlyName = dbEntity.FriendlyName,
                Domain = dbEntity.Domain,

            };

            return Ok(resultDto); // объект без циклов
        }


        // Удалить сущность по EntityId 
        [Authorize(Roles = "Admin")]
        [HttpDelete("{entityId}")]
        public async Task<IActionResult> DeleteEntity(string entityId)
        {
            var exists = await _deviceManager.ExistsAsync(entityId);
            if (!exists)
                return NotFound($"Entity {entityId} not found");

            var deleted = await _deviceManager.DeleteAsync(entityId);
            if (deleted)
                return NoContent();

            return StatusCode(500, "Failed to remove entity");
        }


        // Удалить устройство
        ///

        // Вспомогательный класс для приема данных в POST запросе
        public class DeviceEntityRequest
        {
            public DeviceDtoInputModel Device { get; set; } = null!;

            public EntityDtoInputModel Entity { get; set; } = null!;
        }
    }
}
