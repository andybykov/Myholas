using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Myholas.Core.Dtos
{
    // представление таблицы Devices

    [Table("Devices")]
    public class DeviceEntityDto
    {
        [Key]
        [MaxLength(100)]
        public string EntityId { get; set; } = ""; // "switch.lamp01"


        [MaxLength(100)]
        public string DeviceId { get; set; } = ""; // "esp-lamp01"


        [MaxLength(50)]
        public string Domain { get; set; } = ""; // "switch", "sensor", "light", "select"


        [MaxLength(200)]
        public string? FriendlyName { get; set; } // "Лампа в спальне"


        [MaxLength(100)]
        public string? IpAdress { get; set; } // ip


        [MaxLength(200)]
        public string? CommandTopic { get; set; } // MQTT топик команд


        [MaxLength(200)]
        public string? StateTopic { get; set; } // MQTT топик состояния


        [MaxLength(50)]
        public string? CurrentState { get; set; } // "on", "off", "23.5"


        [MaxLength(50)]
        public string? UnitOfMeasurement { get; set; } // oC


        public string? AttributesJson { get; set; } // Доп. атрибуты 


        public bool IsAvailable { get; set; } = true;  // Доступность


        public DateTime? LastSeen { get; set; }  // Время последнего обновления


        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;


        public DateTime? UpdatedAt { get; set; }


        // Навигационное свойство: история состояний
        public virtual ICollection<StateEntityDto> States { get; set; } = new List<StateEntityDto>();

        public virtual ICollection<UserDeviceAccessDto> UserAccess { get; set; } = new List<UserDeviceAccessDto>();
    }
}
