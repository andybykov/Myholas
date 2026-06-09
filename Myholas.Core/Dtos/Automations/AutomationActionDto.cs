namespace Myholas.Core.Dtos.Automations
{
    public class AutomationActionDto
    {
        // command / delay
        public string Type { get; set; } = "command";

        // switch.fan
        public string EntityId { get; set; } = "";

        // on/off/toggle
        public string Command { get; set; } = "";

        // brightness/delay/etc
        public Dictionary<string, object>? Parameters { get; set; }
    }
}
