using System.Text.Json.Serialization;

namespace Myholas.Core.Dtos.ESPDevices
{
    // для состояний устройств типа light
    public class LightStateAttrDto
    {
        [JsonPropertyName("color_mode")]
        public string? ColorMode { get; set; }


        [JsonPropertyName("state")]
        public string? State { get; set; }


        [JsonPropertyName("color")]
        public object? Color { get; set; }

    }
}
