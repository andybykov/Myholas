using Microsoft.AspNetCore.Mvc;
using Myholas.Core.Interfaces;
using Myholas.Core.Models.Input;
using System.Text.Json;

namespace Myholas.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class AutomationsController : ControllerBase
    {
        private readonly IAutomationManager _manager;
        private readonly IEventBus _eventBus;

        public AutomationsController(IAutomationManager manager, IEventBus eventBus)
        {
            _manager = manager;
            _eventBus = eventBus;
        }

        // Получить все автоматизации
        [HttpGet]
        public async Task<IActionResult> GetAll([FromQuery] bool includeDisabled = false)
        {
            var result = await _manager.GetAllAsync(includeDisabled);
            return Ok(result);
        }

        // Получить одну по ID
        [HttpGet("{id}")]
        public async Task<IActionResult> GetById(int id)
        {
            var automation = await _manager.GetByIdAsync(id);
            if (automation == null) return NotFound();
            return Ok(automation);
        }

        // Создать новую автоматизацию
        [HttpPost]
        public async Task<IActionResult> Create([FromBody] AutomationInputModel dto)
        {
            // Быстрая проверка JSON перед отправкой в менеджер
            if (!IsValidAutomationJson(dto))
                return BadRequest("Invalid or missing JSON configurations");

            // Создаем через менеджер (он сам поставит дату и найдет EntityId)
            var created = await _manager.AddAsync(dto);

            // Уведомляем систему о создании новой автоматизации
            _eventBus.Emit("automation.created", string.Empty);

            return CreatedAtAction(nameof(GetById), new { id = created.Id }, created);
        }

        // Обновить существующую автоматизацию
        [HttpPut("{id}")]
        public async Task<IActionResult> Update(int id, [FromBody] AutomationInputModel dto)
        {
            // Проверка ID из URL и ID из тела запроса
            if (id != dto.Id) return BadRequest("Id mismatch");

            // Быстрая проверка JSON
            if (!IsValidAutomationJson(dto))
                return BadRequest("Invalid or missing JSON configurations");

            // Обновляем через менеджер (он сам поставит UpdatedAt)
            var updated = await _manager.UpdateAsync(id, dto);

            if (updated == null) return NotFound();

            return Ok(updated);
        }

        // Удалить автоматизацию
        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(int id)
        {
            var deleted = await _manager.DeleteAsync(id);
            if (!deleted) return NotFound();
            return NoContent();
        }

        // Переключить статус (Вкл/Выкл)
        [HttpPatch("{id}/toggle")]
        public async Task<IActionResult> Toggle(int id, [FromQuery] bool enabled)
        {
            var updated = await _manager.SetEnabledAsync(id, enabled);
            if (!updated) return NotFound();
            return Ok();
        }

        // Вспомогательный метод для быстрой валидации JSON-полей
        private bool IsValidAutomationJson(AutomationInputModel dto)
        {
            if (string.IsNullOrWhiteSpace(dto.TriggersJson) ||
                string.IsNullOrWhiteSpace(dto.ActionsJson))
                return false;

            if (!IsValidJson(dto.TriggersJson) || !IsValidJson(dto.ActionsJson))
                return false;

            if (!string.IsNullOrWhiteSpace(dto.ConditionsJson) && !IsValidJson(dto.ConditionsJson))
                return false;

            return true;
        }

        private bool IsValidJson(string json)
        {
            try
            {
                JsonDocument.Parse(json);
                return true;
            }
            catch { return false; }
        }
    }
}
