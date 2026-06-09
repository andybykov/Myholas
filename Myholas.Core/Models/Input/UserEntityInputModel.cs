using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using static Myholas.Core.Enums;

namespace Myholas.Core.Models.Input
{
    public class UserEntityInputModel
    {
        [Required(ErrorMessage = "Имя обязательно!!!!")]
        [MaxLength(150, ErrorMessage = "Имя не должно превышать 100 символов")]
        [Display(Name = "Имя")]
        public string UserName { get; set; } = string.Empty;
       
        public string Password { get; set; } = string.Empty;      

        public UserRole Role { get; set; } = UserRole.User;

        public bool IsActive { get; set; } = false;  
    }
}
