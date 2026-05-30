using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Myholas.Core.Dtos.Automations
{
    public class AutomationTriggerDto
    {
        // тип триггера
        // пока только "state"
        public string Type { get; set; } = "state";

        // sensor.temperature
        public string EntityId { get; set; } = "";

        // > < == !=
        public string Operator { get; set; } = "equals";

        // 25
        public string Value { get; set; } = "";
    }
}
