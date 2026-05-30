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
        public int UserId { get; set; }           // Кому дали доступ


        public int DeviceId { get; set; }         // К какому устройству (FK к DeviceDto.Id)


        [MaxLength(20)]
        public string Permission { get; set; }    // "read" или "control"


        public DateTime GrantedAt { get; set; }   // Когда дали


        public int GrantedByUserId { get; set; }  // Кто дал (ID администратора)


        // Навигационные свойства (для EF Core)
        [ForeignKey(nameof(UserId))]
        public virtual UserEntityDto? User { get; set; }


        [ForeignKey(nameof(DeviceId))]
        public virtual DeviceDto? Device { get; set; }
    }
}
