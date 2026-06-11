using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace Myholas.Core.Dtos.DeserializationDtos.ESPDevices
{
    // Конфигурация для выключателя
    public class SwitchEntityConfigDto : BaseEntityConfigDto
    {
        [JsonPropertyName("pl_on")]
        public string? PayloadOn { get; set; } = "ON";


        [JsonPropertyName("pl_off")]
        public string? PayloadOff { get; set; } = "OFF";


        [JsonPropertyName("stat_on")]
        public string? StateOn { get; set; } = "ON";


        [JsonPropertyName("stat_off")]
        public string? StateOff { get; set; } = "OFF";


        [JsonPropertyName("optimistic")]
        public bool? Optimistic { get; set; }
    }
}
