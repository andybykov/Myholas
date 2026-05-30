using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json.Serialization;

namespace Myholas.Core.Dtos.Devices
{
    // представление таблицы States

    [Table("States")]
    public class StateEntityDto
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }

        // [NotMapped] означает, что поле будет в объекте C#, но его НЕ БУДЕТ в таблице БД
        [NotMapped]
        public string EntityIdString { get; set; } = "";

        // привязка к Entity
        public int EntityId { get; set; }
        [ForeignKey(nameof(EntityId))]
        public virtual EntityDto Entity { get; set; } = null!;


        [MaxLength(255)]
        public string? State { get; set; }

        public string? AttributesJson { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    }
}
