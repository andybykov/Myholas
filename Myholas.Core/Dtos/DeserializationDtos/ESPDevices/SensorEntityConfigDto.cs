using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace Myholas.Core.Dtos.DeserializationDtos.ESPDevices
{
    // Конфигурация для сенсора
    public class SensorEntityConfigDto : BaseEntityConfigDto
    {
        [JsonPropertyName("dev_cla")]
        public string? DeviceClass { get; set; }


        [JsonPropertyName("unit_of_meas")]
        public string? UnitOfMeasurement { get; set; }


        [JsonPropertyName("stat_cla")]
        public string? StateClass { get; set; }


        [JsonPropertyName("sug_dsp_prc")]
        public int? SuggestedDisplayPrecision { get; set; }


        [JsonPropertyName("value_template")]
        public string? ValueTemplate { get; set; }
    }

}
