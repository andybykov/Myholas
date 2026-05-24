using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Myholas.Core.Dtos
{
    public class AutomationConditionDto
    {
        public string EntityId { get; set; } = "";

        public string Operator { get; set; } = "==";

        public string Value { get; set; } = "";
    }
}
