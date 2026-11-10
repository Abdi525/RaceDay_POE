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
    public class RaceEntranceController : ControllerBase
    {
        private readonly RaceDayDatabaseContext _context;
        public RaceEntranceController(RaceDayDatabaseContext context) => _context = context;

        [Authorize(Roles = "Participant")]
        [HttpPost]
        [HttpPost("/api/enrolments")]
        public IActionResult Enroll([FromBody] EnrollmentDto dto)
        {
            var userId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);

            var category = _context.EventTypes.Find(dto.EventTypeCode);
            if (category == null) return NotFound(new { message = "Event category not found." });
            var currentCount = _context.RaceEntrances.Count(r => r.EventTypeCode == dto.EventTypeCode);
            if (currentCount >= category.MaxParticipants)
                return BadRequest(new { message = "This event category is full." });
            if (string.IsNullOrWhiteSpace(dto.EmergencyContactNum) &&
                string.IsNullOrWhiteSpace(dto.EmergencyContact))
                return BadRequest(new { message = "An emergency contact number is required." });

            var entrance = new RaceEntrance
            {
                UserId = userId,
                EventTypeCode = dto.EventTypeCode,
                ChipNumber = "CHIP-" + Guid.NewGuid().ToString("N")[..6].ToUpperInvariant(),
                PaymentStatus = "Pending",
                MedicalNotes = dto.MedicalNotes,
                EmergencyContactNum = string.IsNullOrWhiteSpace(dto.EmergencyContactNum)
                    ? dto.EmergencyContact ?? string.Empty
                    : dto.EmergencyContactNum
            };

            _context.RaceEntrances.Add(entrance);
            _context.SaveChanges();
            return StatusCode(201, entrance);
        }

        [Authorize]
        [HttpGet("my")]
        public IActionResult GetMyEntrances()
        {
            var userId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);

            var entries = _context.RaceEntrances
                .Include(r => r.EventType)
                    .ThenInclude(t => t!.Event)
                .Where(r => r.UserId == userId)
                .Select(r => new
                {
                    r.EntranceId, r.EventTypeCode, r.ChipNumber, r.PaymentStatus,
                    r.MedicalNotes, r.EmergencyContactNum,
                    CategoryName = r.EventType!.EventTypeName,
                    EventName = r.EventType.Event!.EventName,
                    EventDate = r.EventType.Event.EventDate,
                    EventLocation = r.EventType.Event.EventLocation
                }).ToList();

            return Ok(entries);
        }

        [Authorize(Roles = "Organiser")]
        [HttpGet("event/{eventId}")]
        [HttpGet("/api/events/{eventId:int}/enrolments")]
        public IActionResult GetEventEnrollments(int eventId)
        {
            var enrollments = _context.RaceEntrances
                .Include(r => r.User)
                .Include(r => r.EventType)
                .Where(r => r.EventType != null && r.EventType.EventId == eventId)
                .Select(r => new
                {
                    r.EntranceId, r.UserId, r.EventTypeCode, r.ChipNumber,
                    r.PaymentStatus, r.MedicalNotes, r.EmergencyContactNum,
                    Participant = r.User == null ? null : new { r.User.FirstName, r.User.LastName, r.User.Email },
                    Category = r.EventType == null ? null : r.EventType.EventTypeName
                }).ToList();
            return Ok(enrollments);
        }
    }

    public class EnrollmentDto
    {
        public int EventTypeCode { get; set; }
        public string? MedicalNotes { get; set; }
        public string EmergencyContactNum { get; set; } = string.Empty;
        public string? EmergencyContact { get; set; }
    }
}
