using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Myholas.Core.Models.Input
{
    public class AutomationInputModel
    {
        public int Id { get; set; } 

        public string EntityId { get; set; } = ""; // "sensor.temp" 

        public string Name { get; set; } = "";

        public string? Description { get; set; }

        public string? TriggersJson { get; set; }

        public string? ConditionsJson { get; set; }

        public string? ActionsJson { get; set; }

        public bool IsEnabled { get; set; } = true;

        public int? CreatedByUserId { get; set; }

        public DateTime? UpdatedAt { get; set; }
    }
}
