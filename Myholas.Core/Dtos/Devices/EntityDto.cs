using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Myholas.Core.Dtos.Devices
{

    [Table("Entities")]
    public class EntityDto
    {
        [Key]
        public int Id { get; set; }

        // Внешний ключ на устройство
        public int DeviceId { get; set; }
        [ForeignKey(nameof(DeviceId))]
        public virtual DeviceDto Device { get; set; } = null!;


        [Required, MaxLength(100)]
        public string EntityId { get; set; } = ""; // "switch.lamp01"


        [MaxLength(50)]
        public string Domain { get; set; } = ""; // "switch", "sensor", "light", "select"


        [MaxLength(200)]
        public string? FriendlyName { get; set; }


        [MaxLength(50)]
        public string? CurrentState { get; set; }


        [MaxLength(50)]
        public string? UnitOfMeasurement { get; set; }


        public string? CommandTopic { get; set; }


        public string? StateTopic { get; set; }


        public string? AttributesJson { get; set; }


        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

        
        public virtual ICollection<StateEntityDto> States { get; set; } = new List<StateEntityDto>();
    }
}
