namespace Myholas.Core.Models.Output
{

    // Группа cущностей, одного физического устройствва 
    public class DeviceOutputModel
    {
        public string DeviceId { get; set; } = ""; // "esp-lamp01"

        public string? FriendlyName { get; set; } // "lamp01"
        
        public string? Version { get; set; }   // прошивка ESPHome

        public string? Ip { get; set; } // "192.168.100.60"

        public DateTime? LastSeen { get; set; }

        public bool? IsOnline { get; set; }  

        public List<EntityOutputModel> Entities { get; set; } = new();
    }


    // Отдельная сущность 
    public class EntityOutputModel
    {
        //public string DeviceId { get; set; } = ""; // "esp-lamp01"

        public string EntityId { get; set; } = ""; // "switch.lamp01"

        public string Domain { get; set; } = ""; // "switch", "sensor", "select", "light"

        public string? Name { get; set; } // "lamp01"

        public string? State { get; set; } // "ON", "23.5", "high"

        public string? UnitOfMeasurement { get; set; } // "oC", "%" 

        public List<string>? Options { get; set; } // для select

        public DateTime? LastSeen { get; set; } // время последенего обновления

        public bool? IsOn { get; set; } // для switch/light
    }
}
