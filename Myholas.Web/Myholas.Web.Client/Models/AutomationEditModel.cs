using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using Myholas.Core.Dtos;

namespace Myholas.Web.Client.Models;

public class AutomationEditModel
{
    public int Id { get; set; }

    public string Name { get; set; } = string.Empty;

    public string? Description { get; set; }

    public bool IsEnabled { get; set; } = true;

    // ======================================================================
    // TRIGGER
    // ======================================================================

    public string TriggerEntityId { get; set; } = string.Empty;

    public string TriggerOperator { get; set; } = "==";

    public string TriggerValue { get; set; } = string.Empty;

    // ======================================================================
    // ACTION
    // ======================================================================

    public string ActionEntityId { get; set; } = string.Empty;

    public string ActionCommandType { get; set; } = "on";

    // ======================================================================
    // DELAY
    // ======================================================================

    public int DelayMs { get; set; }

    // ======================================================================
    // TO DTO
    // ======================================================================

    public AutomationEntityDto ToDto()
    {
        // ----- Triggers ----------------------------------------------------
        var triggers = new List<AutomationTriggerDto>
        {
            new()
            {
                Type = "state",
                EntityId = TriggerEntityId,
                Operator = TriggerOperator,
                Value = TriggerValue
            }
        };

        // ----- Actions -----------------------------------------------------
        var actions = new List<AutomationActionDto>
        {
            new()
            {
                Type = "command",
                EntityId = ActionEntityId,
                Command = ActionCommandType
            }
        };

        // ----- Optional delay ---------------------------------------------
        if (DelayMs > 0)
        {
            actions.Add(new AutomationActionDto
            {
                Type = "delay",
                Parameters = new Dictionary<string, object>
                {
                    ["ms"] = DelayMs
                }
            });
        }

        // ----- Build DTO ---------------------------------------------------
        return new AutomationEntityDto
        {
            Id = Id,
            Name = Name,
            Description = Description,
            IsEnabled = IsEnabled,
            TriggersJson = JsonSerializer.Serialize(
                triggers,
                new JsonSerializerOptions
                {
                    Encoder = System.Text.Encodings.Web.JavaScriptEncoder
                        .UnsafeRelaxedJsonEscaping
                }),
            ActionsJson = JsonSerializer.Serialize(
                actions,
                new JsonSerializerOptions
                {
                    Encoder = System.Text.Encodings.Web.JavaScriptEncoder
                        .UnsafeRelaxedJsonEscaping
                }),
            ConditionsJson = null
        };
    }

    // ======================================================================
    // FROM DTO
    // ======================================================================

    public static AutomationEditModel FromDto(AutomationEntityDto dto)
    {
        var model = new AutomationEditModel
        {
            Id = dto.Id,
            Name = dto.Name,
            Description = dto.Description,
            IsEnabled = dto.IsEnabled
        };

        // ----- Triggers ----------------------------------------------------
        if (!string.IsNullOrWhiteSpace(dto.TriggersJson))
        {
            var triggers = JsonSerializer.Deserialize<List<AutomationTriggerDto>>(dto.TriggersJson);
            var trigger = triggers?.FirstOrDefault();

            if (trigger != null)
            {
                model.TriggerEntityId = trigger.EntityId;
                model.TriggerOperator = trigger.Operator;
                model.TriggerValue = trigger.Value;
            }
        }

        // ----- Actions -----------------------------------------------------
        if (!string.IsNullOrWhiteSpace(dto.ActionsJson))
        {
            var actions = JsonSerializer.Deserialize<List<AutomationActionDto>>(dto.ActionsJson);

            // Command
            var commandAction = actions?.FirstOrDefault(a => a.Type == "command");
            if (commandAction != null)
            {
                model.ActionEntityId = commandAction.EntityId;
                model.ActionCommandType = commandAction.Command;
            }

            // Delay
            var delayAction = actions?.FirstOrDefault(a => a.Type == "delay");
            if (delayAction?.Parameters != null &&
                delayAction.Parameters.TryGetValue("ms", out var delay))
            {
                model.DelayMs = Convert.ToInt32(delay);
            }
        }

        return model;
    }
}
