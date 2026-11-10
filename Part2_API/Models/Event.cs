namespace poe11.Models
{
    public class Event
    {
        public int EventId { get; set; }
        public int OrganizerId { get; set; }
        public Users? Organizer { get; set; }
        public string EventName { get; set; } = string.Empty;
        public string EventLocation { get; set; } = string.Empty;
        public DateTime EventDate { get; set; }
        public string? EventDescription { get; set; }
        public string? ImagePath { get; set; }
        public ICollection<EventTypes> EventTypes { get; set; } = new List<EventTypes>();
    }
}
