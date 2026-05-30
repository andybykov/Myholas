using System.Text.Json.Serialization;

namespace Myholas.Core.Dtos.ESPDevices
{
    // Первично найденное устройство ESPhome
    // временное представление 
    public sealed class EspDeviceDto
    {
        [JsonPropertyName("ip")]
        public string? Ip { get; set; }


        [JsonPropertyName("name")] 
        public string? Name { get; set; } // == DeviceId


        [JsonPropertyName("friendly_name")]
        public string? FriendlyName { get; set; }


        [JsonPropertyName("version")]
        public string? Version { get; set; }


        [JsonPropertyName("mac")]
        public string? Mac { get; set; }


        [JsonPropertyName("platform")]
        public string? Platform { get; set; }


        [JsonPropertyName("board")]
        public string? Board { get; set; }


        [JsonPropertyName("network")]
        public string? Network { get; set; }

        // Конфиги сущностей устройства
        public List<BaseEntityConfigDto> Entities { get; set; } = new();
    }    
}
