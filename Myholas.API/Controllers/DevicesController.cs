using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Myholas.BLL;
using Myholas.Core.Dtos;
using Myholas.Core.Interfaces;
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

        //  Получить все устройства в виде списка EntityOutputModel 
        [HttpGet("entities")]
        public async Task<ActionResult<List<EntityOutputModel>>> GetAllEntities([FromQuery] bool includeUnavailable = false)
        {
            var entities = await _deviceManager.GetAllEntitiesAsync(includeUnavailable);

            return Ok(entities);
        }

        //  Получить устройство по EntityId в виде EntityOutputModel 
        [HttpGet("entities/{entityId}")]
        public async Task<ActionResult<EntityOutputModel>> GetEntityById(string entityId)
        {
            var entity = await _deviceManager.GetEntityByIdAsync(entityId);
            if (entity == null)
                return NotFound($"Device {entityId} not found");

            return Ok(entity);
        }

        //  Получить устройства по домену (switch, sensor, light, select) 
        [HttpGet("entities/by-domain/{domain}")]
        public async Task<ActionResult<List<EntityOutputModel>>> GetEntitiesByDomain(string domain)
        {
            var entities = await _deviceManager.GetEntitiesByDomainAsync(domain);
            return Ok(entities);
        }

        //  Получить устройства по физическому DeviceId 
        [HttpGet("entities/by-device/{deviceId}")]
        public async Task<ActionResult<List<EntityOutputModel>>> GetEntitiesByDeviceId(string deviceId)
        {
            var entities = await _deviceManager.GetEntitiesByDeviceIdAsync(deviceId);

            return Ok(entities);
        }

        //  Получить группированные устройства 
        [HttpGet("groups")]
        public async Task<ActionResult<List<DeviceOutputModels>>> GetGroupedDevices()
        {
            var groups = await _deviceManager.GetGroupedDevicesAsync();

            return Ok(groups);
        }

        //  CRUD 

        //  Добавить или обновить устройство 
        [HttpPost]
        public async Task<ActionResult<DeviceEntityDto>> AddOrUpdate([FromBody] DeviceEntityDto device)
        {
            if (string.IsNullOrWhiteSpace(device.EntityId))
                return BadRequest("EntityId is required");

            var result = _deviceManager.AddOrUpdateAsync(device);
            return Ok(result);
        }

        //  Удалить устройство 
        [HttpDelete("{entityId}")]
        public async Task<IActionResult> Delete(string entityId)
        {
            var exists = await _deviceManager.ExistsAsync(entityId);
            if (!exists)
                return NotFound($"Device {entityId} not found");
            var deleted = await _deviceManager.DeleteAsync(entityId);
            if (deleted)
                return NoContent();

            return StatusCode(500, "Failed to remove device");
        }
    }
}