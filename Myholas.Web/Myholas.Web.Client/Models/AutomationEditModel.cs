using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using Myholas.Core.Dtos.Automations;
using Myholas.Core.Models.Input; // Добавляем для AutomationInputModel
using Myholas.Core.Models.Output; // Добавляем для AutomationOutputModel

namespace Myholas.Web.Client.Models
{
    public class AutomationEditModel
    {
        public int Id { get; set; }

        public string Name { get; set; } = string.Empty;

        public string? Description { get; set; }

        public bool IsEnabled { get; set; } = true;


        // TRIGGER
        public string TriggerEntityId { get; set; } = string.Empty;

        public string TriggerOperator { get; set; } = "equals";

        public string TriggerValue { get; set; } = string.Empty;


        // CONDITIONS 
        public List<ConditionModel> Conditions { get; set; } = new();

        // ACTION
        public string ActionEntityId { get; set; } = string.Empty;

        public string ActionCommandType { get; set; } = "on";

        public int DelayMs { get; set; }

        // to InputModel
        public AutomationInputModel ToInputModel()
        {
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

            var actions = new List<AutomationActionDto>
            {
                new()
                {
                    Type = "command",
                    EntityId = ActionEntityId,
                    Command = ActionCommandType
                }
            };

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

            return new AutomationInputModel
            {
                Id = Id,
                Name = Name,
                Description = Description,
                IsEnabled = IsEnabled,
                EntityId = TriggerEntityId,
                TriggersJson = JsonSerializer.Serialize(triggers),
                ActionsJson = JsonSerializer.Serialize(actions),                
                ConditionsJson = Conditions.Any()
                ? JsonSerializer.Serialize(Conditions.Select(c => new AutomationConditionDto
                {
                    EntityId = c.EntityId,
                    Operator = c.Operator,
                    Value = c.Value
                }))
                : null
            };
        }

        // FROM UTPUT MODEL for edit
        public static AutomationEditModel FromOutputModel(AutomationOutputModel dto)
        {
            var model = new AutomationEditModel
            {
                Id = dto.Id,
                Name = dto.Name,
                Description = dto.Description,
                IsEnabled = dto.IsEnabled
            };

            var trigger = dto.Triggers?.FirstOrDefault();
            if (trigger != null)
            {
                model.TriggerEntityId = trigger.EntityId;
                model.TriggerOperator = trigger.Operator;
                model.TriggerValue = trigger.Value;
            }

            var commandAction = dto.Actions?.FirstOrDefault(a => a.Type == "command");
            if (commandAction != null)
            {
                model.ActionEntityId = commandAction.EntityId;
                model.ActionCommandType = commandAction.Command;
            }

            var delayAction = dto.Actions?.FirstOrDefault(a => a.Type == "delay");
            if (delayAction?.Parameters != null &&
                delayAction.Parameters.TryGetValue("ms", out var delay))
            {
                model.DelayMs = Convert.ToInt32(delay);
            }

            model.Conditions = dto.Conditions?.Select(c => new ConditionModel
            {
                EntityId = c.EntityId,
                Operator = c.Operator,
                Value = c.Value
            }).ToList() ?? new();
  

            return model;
        }
    }

    // Вспомогательный класс 
    public class ConditionModel 
    {
        public string EntityId { get; set; } = "";

        public string Operator { get; set; } = "equals";

        public string Value { get; set; } = "";
    }
}
