namespace Myholas.Core.Models.Output
{
    public class DeviceHistoryOutputModel
    {
        public int Id { get; set; }


        public string EntityId { get; set; } = "";


        public string? State { get; set; }


        public DateTime CreatedAt { get; set; }


        public string? AttributesSummary { get; set; }
    }
}
