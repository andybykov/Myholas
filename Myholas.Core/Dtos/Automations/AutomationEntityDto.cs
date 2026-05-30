using Myholas.Core.Dtos.Devices;
using Myholas.Core.Dtos.Users;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Myholas.Core.Dtos.Automations
{
    // представление таблицы Automations 

    [Table("Automations")]
    public class AutomationEntityDto
    {
        [Key]
        public int Id { get; set; }

        // ПРИВЯЗКА К СУЩНОСТИ
        public int EntityId { get; set; }
        [ForeignKey(nameof(EntityId))]

        public virtual EntityDto Entity { get; set; } = null!;

        public int? CreatedByUserId { get; set; }

        [MaxLength(200)]
        public string Name { get; set; } = "";

        [MaxLength(500)]
        public string? Description { get; set; }

        public string? TriggersJson { get; set; }

        public string? ConditionsJson { get; set; }

        public string? ActionsJson { get; set; }

        public bool IsEnabled { get; set; } = true;

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        public DateTime? UpdatedAt { get; set; }

        public virtual UserEntityDto? CreatedByUser { get; set; }
    }
}
