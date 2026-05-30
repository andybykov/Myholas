using Myholas.Core.Dtos.Automations;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Myholas.Core.Models.Output
{
    public class AutomationOutputModel
    {
        public int Id { get; set; }

        public string Name { get; set; } = "";

        public string? Description { get; set; }

        public bool IsEnabled { get; set; }


        // Вместо int EntityId возвращаем строку "sensor.temp"
        public string EntityId { get; set; } = "";


        // Имя пользователя, который создал автоматизацию
        public string? CreatedByUserName { get; set; }

        public DateTime CreatedAt { get; set; }

        public DateTime? UpdatedAt { get; set; }

        // Вместо JSON-строк возвращаем уже готовые списки объектов
        public List<AutomationTriggerDto> Triggers { get; set; } = new();

        public List<AutomationConditionDto> Conditions { get; set; } = new();

        public List<AutomationActionDto> Actions { get; set; } = new();
    }
}
