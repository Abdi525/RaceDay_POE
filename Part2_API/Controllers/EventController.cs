using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using poe11.Models;
using poe11.RaceDayDatabase;

namespace poe11.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class EventsController : ControllerBase
    {
        private readonly RaceDayDatabaseContext _context;
        public EventsController(RaceDayDatabaseContext context) => _context = context;

        [HttpGet]
        public IActionResult GetEvents()
        {
            var events = _context.Events
                .Include(e => e.EventTypes)
                .OrderBy(e => e.EventDate)
                .Select(e => new
                {
                    e.EventId, e.OrganizerId, e.EventName, e.EventLocation,
                    e.EventDate, e.EventDescription, e.ImagePath,
                    EventTypes = e.EventTypes!.Select(t => new
                    {
                        t.EventTypeCode, t.EventId, t.EventTypeName,
                        t.DistanceKM, t.EntranceFee, t.MaxParticipants
                    }).ToList()
                }).ToList();

            return Ok(events);
        }

        [HttpGet("{id:int}")]
        public IActionResult GetEvent(int id)
        {
            var ev = _context.Events
                .Include(e => e.EventTypes)
                .Where(e => e.EventId == id)
                .Select(e => new
                {
                    e.EventId, e.OrganizerId, e.EventName, e.EventLocation,
                    e.EventDate, e.EventDescription, e.ImagePath,
                    EventTypes = e.EventTypes.Select(t => new
                    {
                        t.EventTypeCode, t.EventId, t.EventTypeName,
                        t.DistanceKM, t.EntranceFee, t.MaxParticipants
                    }).ToList()
                }).FirstOrDefault();

            return ev == null ? NotFound() : Ok(ev);
        }

        [Authorize(Roles = "Organiser")]
        [HttpPost]
        public IActionResult CreateEvent([FromBody] EventDto dto)
        {
            var userId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);

            var newEvent = new Event
            {
                OrganizerId = userId,
                EventName = dto.EventName,
                EventLocation = dto.EventLocation,
                EventDate = dto.EventDate,
                EventDescription = dto.EventDescription,
                ImagePath = dto.ImagePath
            };

            _context.Events.Add(newEvent);
            _context.SaveChanges();
            return StatusCode(201, newEvent);
        }

        [Authorize(Roles = "Organiser")]
        [HttpPut("{id}")]
        public IActionResult UpdateEvent(int id, [FromBody] EventDto dto)
        {
            var ev = _context.Events.Find(id);
            if (ev == null) return NotFound(new { message = "Event not found." });

            ev.EventName = dto.EventName;
            ev.EventLocation = dto.EventLocation;
            ev.EventDate = dto.EventDate;
            ev.EventDescription = dto.EventDescription;
            ev.ImagePath = dto.ImagePath ?? ev.ImagePath;
            _context.SaveChanges();
            return Ok(ev);
        }

        [Authorize(Roles = "Organiser")]
        [HttpDelete("{id}")]
        public IActionResult DeleteEvent(int id)
        {
            var ev = _context.Events.Find(id);
            if (ev == null) return NotFound(new { message = "Event not found." });
            _context.Events.Remove(ev);
            _context.SaveChanges();
            return NoContent();
        }
    }

    public class EventDto
    {
        public string EventName { get; set; } = string.Empty;
        public string EventLocation { get; set; } = string.Empty;
        public DateTime EventDate { get; set; }
        public string? EventDescription { get; set; }
        public string? ImagePath { get; set; }
    }
}
