using Myholas.Core.Dtos.DeserializationDtos.ESPDevices;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace Myholas.Core.Dtos.DeserializationDtos.Z2mDevices
{
    public sealed class Z2MDeviceDto
    {
        [JsonPropertyName("friendly_name")]
        public string FriendlyName { get; set; } = "";


        [JsonPropertyName("ieee_address")]
        public string IeeeAddress { get; set; } = ""; // Это и есть физический DeviceId


        [JsonPropertyName("model")]
        public string? Model { get; set; }


        public bool IsOnline { get; set; }


        [JsonPropertyName("exposes")]
        public List<Z2MExposeDto> Exposes { get; set; } = new();

       
    }

    public sealed class Z2MExposeDto
    {
        [JsonPropertyName("name")]
        public string Name { get; set; } = ""; // Например: "temperature", "battery"

        [JsonPropertyName("label")]
        public string? Label { get; set; } // Например: "Температура"

        [JsonPropertyName("type")]
        public string? Type { get; set; } // "numeric", "binary", "enum"
    }
}

