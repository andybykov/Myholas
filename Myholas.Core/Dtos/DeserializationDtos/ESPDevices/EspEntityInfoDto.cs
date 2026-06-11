using System.Text.Json.Serialization;

namespace Myholas.Core.Dtos.DeserializationDtos.ESPDevices
{
    // соответствует блоку "dev" Home Assistant MQTT Discovery
    /*
    "dev": {
    "ids": "8caab5f445a8",
    "name": "heater",
    "mf": "Espressif",
    "mdl": "esp01_1m",
    "sw": "2026.4.4",
    "cns": [["mac","8caab5f445a8"]]
    }
    */
    public class EspEntityInfoDto
    {
        [JsonPropertyName("ids")] public string? Identifiers { get; set; }


        [JsonPropertyName("name")] public string? Name { get; set; }


        [JsonPropertyName("mf")] public string? Manufacturer { get; set; }


        [JsonPropertyName("mdl")] public string? Model { get; set; }


        [JsonPropertyName("sw")] public string? SoftwareVersion { get; set; }


        [JsonPropertyName("cns")] public List<List<string>>? Connections { get; set; }
    }
}

