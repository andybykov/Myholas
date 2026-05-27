using Microsoft.AspNetCore.Mvc;
using Myholas.BLL;
using Myholas.Core.Interfaces;
using Myholas.Core.Models.Output;

namespace Myholas.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class StatesController : ControllerBase
    {
        private readonly IStateManager _stateManager;

        private readonly ICommandService _commandService;


        public StatesController(IStateManager stateManager, ICommandService commandService)
        {
            _stateManager = stateManager;
            _commandService = commandService;
        }

        //  Получить текущее состояние устройства 
        [HttpGet("{entityId}/current")]
        public async Task<ActionResult<EntityOutputModel>> GetCurrentState(string entityId)
        {
            var state = await _stateManager.GetCurrentStateAsync(entityId);
            if (state == null)
                return NotFound($"States for {entityId} not found");

            return Ok(state);
        }

        //  Получить историю состояний 
        [HttpGet("{entityId}/history")]
        public async Task<ActionResult<List<DeviceHistoryOutputModel>>> GetHistory(
            string entityId,
            [FromQuery] DateTime? from,
            [FromQuery] DateTime? to,
            [FromQuery] int limit = 100)
        {
            var history = await _stateManager.GetHistoryAsync(entityId, from, to, limit);

            return Ok(history);
        }

        //  Отправить команду устройству 
        [HttpPost("{entityId}/command")]
        public async Task<IActionResult> SendCommand(string entityId, [FromBody] string command)
        {
            try
            {
                await _commandService.SendCommandAsync(entityId, command);

                return Accepted();
            }
            catch (InvalidOperationException ex)
            {
                return NotFound(ex.Message);
            }
        }

        //  Включить устройство 
        [HttpPost("{entityId}/turn_on")]
        public async Task<IActionResult> TurnOn(string entityId)
        {
            await _commandService.SendCommandAsync(entityId, "on");
            return Accepted();
        }

        //  Выключить устройство 
        [HttpPost("{entityId}/turn_off")]
        public async Task<IActionResult> TurnOff(string entityId)
        {
            await _commandService.SendCommandAsync(entityId, "off");
            return Accepted();
        }

        //  Переключить устройство 
        [HttpPost("{entityId}/toggle")]
        public async Task<IActionResult> Toggle(string entityId)
        {
            await _commandService.SendCommandAsync(entityId, "toggle");
            return Accepted();
        }

        //  Установить яркость 
        [HttpPost("{entityId}/brightness")]
        public async Task<IActionResult> SetBrightness(string entityId, [FromBody] int brightness)
        {
            await _commandService.SendCommandAsync(entityId, "brightness", new { brightness });
            return Accepted();
        }
    }
}