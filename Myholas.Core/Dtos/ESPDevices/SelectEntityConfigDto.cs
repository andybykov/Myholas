using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace Myholas.Core.Dtos.ESPDevices
{
    // Конфигурация для select
    public class SelectEntityConfigDto : BaseEntityConfigDto
    {
        [JsonPropertyName("ops")]
        public string[]? Options { get; set; }  


        [JsonPropertyName("optimistic")]
        public bool? Optimistic { get; set; }
    }
}
