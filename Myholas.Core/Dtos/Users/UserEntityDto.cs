using Myholas.Core.Dtos.Automations;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static Myholas.Core.Enums;

namespace Myholas.Core.Dtos.Users
{
    [Table("Users")]
    public class UserEntityDto
    {
        [Key]
        public int Id { get; set; }


        [MaxLength(100)]
        public string Username { get; set; } = "";


//        [MaxLength(255)]
//        public string Email { get; set; } = "";


        [MaxLength(255)]
        public string PasswordHash { get; set; } = "";   // Хеш пароля


        public UserRole Role { get; set; } = UserRole.User;      


        public bool IsActive { get; set; } = true;


        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;


        public DateTime? LastLogin { get; set; }


        public virtual ICollection<UserDeviceAccessDto> DeviceAccess { get; set; } = new List<UserDeviceAccessDto>();

        public virtual ICollection<AutomationEntityDto> Automations { get; set; } = new List<AutomationEntityDto>();

    }    
}
