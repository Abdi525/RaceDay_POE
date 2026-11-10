namespace poe11.Models
{
    public class EventTypes
    {
        public int EventTypeCode { get; set; }
        public int EventId { get; set; }
        public Event? Event { get; set; }
        public string EventTypeName { get; set; } = string.Empty; 
        public decimal DistanceKM { get; set; }
        public decimal EntranceFee { get; set; }
        public int MaxParticipants { get; set; }
    }
}
