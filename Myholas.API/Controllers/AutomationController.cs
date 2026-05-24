using Microsoft.AspNetCore.Mvc;
using Myholas.Core.Dtos;
using Myholas.Core.Interfaces;
using System.Text.Json;

namespace Myholas.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class AutomationsController : ControllerBase
    {
        private readonly IAutomationManager _manager;

        public AutomationsController(
            IAutomationManager manager)
        {
            _manager = manager;
        }

        [HttpGet]
        public async Task<IActionResult> GetAll([FromQuery] bool includeDisabled = false)
        {
            var result = await _manager.GetAllAsync(includeDisabled);

            return Ok(result);
        }

        // Add

        [HttpGet("{id}")]
        public async Task<IActionResult> GetById(int id)
        {
            var automation = await _manager.GetByIdAsync(id);

            if (automation == null)
                return NotFound();

            return Ok(automation);
        }

        // Create
        [HttpPost]
        public async Task<IActionResult> Create([FromBody] AutomationEntityDto dto)
        {
            // VALIDATION

            if (string.IsNullOrWhiteSpace(dto.Name))
            {
                return BadRequest(
                    "Name is required");
            }

            if (string.IsNullOrWhiteSpace(dto.TriggersJson))
            {
                return BadRequest(
                    "TriggersJson is required");
            }

            if (string.IsNullOrWhiteSpace(dto.ActionsJson))
            {
                return BadRequest(
                    "ActionsJson is required");
            }

            // JSON VALIDATION

            if (!IsValidJson(dto.TriggersJson))
            {
                return BadRequest(
                    "TriggersJson is invalid");
            }

            if (!IsValidJson(dto.ActionsJson))
            {
                return BadRequest(
                    "ActionsJson is invalid");
            }

            if (!string.IsNullOrWhiteSpace(
                dto.ConditionsJson))
            {
                if (!IsValidJson(dto.ConditionsJson))
                {
                    return BadRequest(
                        "ConditionsJson is invalid");
                }
            }

            // CREATE

            dto.CreatedAt = DateTime.UtcNow;

            var created =
                await _manager.AddAsync(dto);

            return CreatedAtAction(nameof(GetById), new { id = created.Id }, created);
        }


        // UPDATE

        [HttpPut("{id}")]
        public async Task<IActionResult> Update(int id, [FromBody] AutomationEntityDto dto)
        {
            if (id != dto.Id)
            {
                return BadRequest("Id mismatch");
            }

            var existing = await _manager.GetByIdAsync(id);

            if (existing == null)
            {
                return NotFound();
            }

            // VALIDATION

            if (string.IsNullOrWhiteSpace(dto.Name))
            {
                return BadRequest("Name is required");
            }

            if (string.IsNullOrWhiteSpace(dto.TriggersJson))
            {
                return BadRequest("TriggersJson is required");
            }

            if (string.IsNullOrWhiteSpace(dto.ActionsJson))
            {
                return BadRequest("ActionsJson is required");
            }

            // JSON VALIDATION

            if (!IsValidJson(dto.TriggersJson))
            {
                return BadRequest("TriggersJson is invalid");
            }

            if (!IsValidJson(dto.ActionsJson))
            {
                return BadRequest("ActionsJson is invalid");
            }

            if (!string.IsNullOrWhiteSpace(
                dto.ConditionsJson))
            {
                if (!IsValidJson(dto.ConditionsJson))
                {
                    return BadRequest("ConditionsJson is invalid");
                }
            }

            dto.UpdatedAt = DateTime.UtcNow;

            await _manager.UpdateAsync(id, dto);

            return Ok(dto);
        }


        // DELETE       

        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(int id)
        {
            var deleted = await _manager.DeleteAsync(id);

            if (!deleted)
                return NotFound();

            return NoContent();
        }


        // ENABLE / DISABLE

        [HttpPatch("{id}/toggle")]
        public async Task<IActionResult> Toggle(int id, [FromQuery] bool enabled)
        {
            var updated = await _manager.SetEnabledAsync(id, enabled);

            if (!updated)
                return NotFound();

            return Ok();
        }


        // JSON VALIDATION

        private bool IsValidJson(string json)
        {
            try
            {
                JsonDocument.Parse(json);

                return true;
            }
            catch
            {
                return false;
            }
        }
    }
}