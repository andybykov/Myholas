using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Myholas.Core.Dtos
{
    [Table("UserDevices")]
    public class UserDeviceAccessDto
    {
        public int UserId { get; set; }           // Кому дали доступ

        public string DeviceId { get; set; }      // К какому устройству

        public string Permission { get; set; }    // "read" или "control"

        public DateTime GrantedAt { get; set; }   // Когда дали

        public int GrantedByUserId { get; set; }  // Кто дал (ID администратора)


        // Навигационные свойства (для EF Core)
        public virtual UserEntityDto? User { get; set; }              // Пользователь, получивший доступ

        public virtual DeviceEntityDto? Device { get; set; }         // Устройство

     }
}
