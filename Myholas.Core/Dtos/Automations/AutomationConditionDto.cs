namespace Myholas.Core.Dtos.Automations
{
    public class AutomationConditionDto
    {
        public string EntityId { get; set; } = "";

        public string Operator { get; set; } = "equals";

        public string Value { get; set; } = "";
    }
}
