using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Myholas.Core.Models.Input
{
    public class EntityDtoInputModel
    {
        [Required]
        [MaxLength(100)]
        public string EntityId { get; set; } = ""; 


        [Required]
        [MaxLength(50)]
        public string Domain { get; set; } = "";


        [MaxLength(200)]
        public string? FriendlyName { get; set; } = "";
    }
}
