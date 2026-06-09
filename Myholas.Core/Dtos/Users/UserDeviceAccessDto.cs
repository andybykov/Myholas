using Myholas.Core.Dtos.Devices;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Myholas.Core.Dtos.Users
{
    [Table("UserDevices")]
    public class UserDeviceAccessDto
    {
        // В составном ключе используются ID (int)
        public int UserId { get; set; }  


        public int DeviceId { get; set; } // FK к DeviceDto.Id


        [MaxLength(20)]
        public string Permission { get; set; }   


        public DateTime GrantedAt { get; set; }  


        public int GrantedByUserId { get; set; }  

        // Навигационные свойства 
        [ForeignKey(nameof(UserId))]
        public virtual UserEntityDto? User { get; set; }


        [ForeignKey(nameof(DeviceId))]
        public virtual DeviceDto? Device { get; set; }
    }
}
