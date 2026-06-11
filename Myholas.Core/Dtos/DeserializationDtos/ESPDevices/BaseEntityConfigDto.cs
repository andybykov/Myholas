using System.Text.Json.Serialization;

namespace Myholas.Core.Dtos.DeserializationDtos.ESPDevices
{

    // полиморфная десриализация
    [JsonDerivedType(typeof(SwitchEntityConfigDto), typeDiscriminator: "switch")]

    [JsonDerivedType(typeof(SelectEntityConfigDto), typeDiscriminator: "select")]

    [JsonDerivedType(typeof(SensorEntityConfigDto), typeDiscriminator: "sensor")]

    [JsonDerivedType(typeof(LightEntityConfigDto), typeDiscriminator: "light")]

    // Базовый класс для всех конфигураций
    public abstract class BaseEntityConfigDto
    {
        
        public string? Domain { get; set; }
 
        public string? ObjectId { get; set; } //lamp01

        public string? EntityId { get; set; }

        public string? EspName { get; set; }


        [JsonPropertyName("name")]
        public string? Name { get; set; }


        [JsonPropertyName("uniq_id")]
        public string? UniqueId { get; set; }


        [JsonPropertyName("stat_t")]
        public string? StateTopic { get; set; }


        [JsonPropertyName("cmd_t")]
        public string? CommandTopic { get; set; }


        [JsonPropertyName("avty_t")]
        public string? AvailabilityTopic { get; set; }


        [JsonPropertyName("pl_avail")]
        public string? PayloadAvailable { get; set; } = "online";


        [JsonPropertyName("pl_not_avail")]
        public string? PayloadNotAvailable { get; set; } = "offline";


        [JsonPropertyName("dev")]
        public EspEntityInfoDto? Device { get; set; }


        [JsonPropertyName("qos")]
        public int Qos { get; set; } = 0;


        [JsonPropertyName("icon")]
        public string? Icon { get; set; }


        [JsonPropertyName("entity_category")]
        public string? EntityCategory { get; set; }
       
    }

}
