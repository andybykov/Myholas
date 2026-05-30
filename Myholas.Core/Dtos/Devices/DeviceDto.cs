using Myholas.Core.Dtos.Users;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Myholas.Core.Dtos.Devices
{
    // представление таблицы Devices

    [Table("Devices")]
    public class DeviceDto
    {
        [Key]
        public int Id { get; set; }


        [Required, MaxLength(100)]
        public string DeviceId { get; set; } = ""; // Физический ID: "esp-lamp01"


        [MaxLength(200)]
        public string? FriendlyName { get; set; }


        [MaxLength(100)]
        public string? IpAddress { get; set; }


        public string? Version { get; set; }


        public DateTime? LastSeen { get; set; }


        public bool IsOnline { get; set; }


        public virtual ICollection<EntityDto> Entities { get; set; } = new List<EntityDto>();


        // Много записей о доступе пользователей
        public virtual ICollection<UserDeviceAccessDto> UserAccess { get; set; } = new List<UserDeviceAccessDto>();
    }
}

