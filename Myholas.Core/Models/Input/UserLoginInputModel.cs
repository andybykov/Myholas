using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Myholas.Core.Models.Input
{
    public class UserLoginInputModel
    {
        [Required(ErrorMessage = "Имя обязательно")]
        public  string Username { get; set; } = string.Empty;


        [Required(ErrorMessage = "Пароль обязателен")]
        public string Password { get; set; } = string.Empty;
    }
}
