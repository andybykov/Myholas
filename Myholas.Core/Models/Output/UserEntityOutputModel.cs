using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static Myholas.Core.Enums;

namespace Myholas.Core.Models.Output
{
    public class UserEntityOutputModel
   {
        public required string Username { get; set; }

        public UserRole Role { get; set; } 


        public bool IsActive { get; set; }


        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;


        public DateTime? LastLogin { get; set; }

    }
}

