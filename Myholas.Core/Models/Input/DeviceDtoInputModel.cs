using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Myholas.Core.Models.Input
{
    public class DeviceDtoInputModel
    {
        [Required]
        [MaxLength(100)]
        public string DeviceId { get; set; } = "";


        [MaxLength(200)]
        public string? FriendlyName { get; set; }

    }

}
