using System.Text.Json.Serialization;

namespace Myholas.Core.Dtos.ESPDevices
{
    // Конфигурация для света
    public class LightEntityConfigDto : BaseEntityConfigDto
    {
        [JsonPropertyName("bri_stat_t")]
        public string? BrightnessStateTopic { get; set; }


        [JsonPropertyName("bri_cmd_t")]
        public string? BrightnessCommandTopic { get; set; }


        [JsonPropertyName("rgb_stat_t")]
        public string? RgbStateTopic { get; set; }


        [JsonPropertyName("rgb_cmd_t")]
        public string? RgbCommandTopic { get; set; }


        [JsonPropertyName("effect_list")]
        public string[]? EffectList { get; set; }
    }
}
