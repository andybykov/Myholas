using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Myholas.Core.Dtos
{
    // представление таблицы Automations 

    [Table("Automations")]
    public class AutomationEntityDto
    {
        [Key]
        public int Id { get; set; }

        public int? CreatedByUserId { get; set; }  // ID пользователя, создавшего автоматизацию


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

        // навигация
        public virtual UserEntityDto? CreatedByUser { get; set; } 
    }
}
