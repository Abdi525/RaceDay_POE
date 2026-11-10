using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using poe11.Models;
using poe11.RaceDayDatabase;

namespace poe11.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class EventTypesController : ControllerBase
    {
        private readonly RaceDayDatabaseContext _context;
        public EventTypesController(RaceDayDatabaseContext context) => _context = context;

        [HttpGet]
        [HttpGet("/api/events/{eventId:int}/categories")]
        public IActionResult GetCategories([FromQuery] int eventId)
        {
            var categories = _context.EventTypes.Where(c => c.EventId == eventId).ToList();
            return Ok(categories);
        }

        [Authorize(Roles = "Organiser")]
        [HttpPost]
        [HttpPost("/api/events/{eventId:int}/categories")]
        public IActionResult AddCategory([FromQuery] int eventId, [FromBody] EventTypeDto dto)
        {
            var ev = _context.Events.Find(eventId);
            if (ev == null) return NotFound(new { message = "Event not found." });

            var category = new EventTypes
            {
                EventId = eventId,
                EventTypeName = dto.EventTypeName,
                DistanceKM = dto.DistanceKM,
                EntranceFee = dto.EntranceFee,
                MaxParticipants = dto.MaxParticipants
            };
            _context.EventTypes.Add(category);
            _context.SaveChanges();
            return StatusCode(201, category);
        }
    }

    public class EventTypeDto
    {
        public string EventTypeName { get; set; } = string.Empty;
        public decimal DistanceKM { get; set; }
        public decimal EntranceFee { get; set; }
        public int MaxParticipants { get; set; }
    }
}
