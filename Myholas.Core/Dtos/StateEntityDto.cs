using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json.Serialization;

namespace Myholas.Core.Dtos
{
    // представление таблицы States

    [Table("States")]
    public class StateEntityDto
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }


        [MaxLength(100)]
        public string EntityId { get; set; } = "";


        [MaxLength(255)]
        public string? State { get; set; }


        public string? AttributesJson { get; set; } // JSON дополнительных атрибутов 


        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;


        [ForeignKey(nameof(EntityId))]    
        public virtual DeviceEntityDto? Device { get; set; }
    }
}
