namespace poe11.Models
{
    public class RaceEntrance
    {
        public int EntranceId { get; set; }
        public int UserId { get; set; }
        public Users? User { get; set; }
        public int EventTypeCode { get; set; }
        public EventTypes? EventType { get; set; }
        public string? ChipNumber { get; set; }
        public string PaymentStatus { get; set; } = "Pending"; 
        public string? MedicalNotes { get; set; }
        public string EmergencyContactNum { get; set; } = string.Empty;
    }
}
