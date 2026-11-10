namespace poe11.Models
{
    public class RouteMap
    {
        public int RouteId { get; set; }
        public int EventTypeCode { get; set; }
        public EventTypes? EventType { get; set; }
        public DateTime MapDate { get; set; } = DateTime.Now; 
        public string StartPoint { get; set; } = string.Empty; 
        public string EndPoint { get; set; } = string.Empty; 
        public string? RouteDescription { get; set; }
    }
}