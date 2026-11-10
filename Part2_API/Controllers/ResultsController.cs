using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using poe11.RaceDayDatabase;
using RaceResult = poe11.Models.Results;

namespace poe11.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ResultsController : ControllerBase
    {
        private readonly RaceDayDatabaseContext _context;
        public ResultsController(RaceDayDatabaseContext context) => _context = context;

        [Authorize(Roles = "Organiser")]
        [HttpPost]
        public IActionResult CaptureResult([FromBody] ResultDto dto)
        {
            if (!_context.RaceEntrances.Any(e => e.EntranceId == dto.EntranceId))
                return NotFound(new { message = "Entrance ID not found." });
            if (_context.Results.Any(r => r.EntranceId == dto.EntranceId))
                return Conflict(new { message = "A result already exists for this entrance." });

            var result = new RaceResult
            {
                EntranceId = dto.EntranceId,
                FinishStatus = dto.FinishStatus,
                OverallRank = dto.OverallRank,
                CategoryRank = dto.CategoryRank,
                AgeGroupRank = dto.AgeGroupRank
            };
            _context.Results.Add(result);
            _context.SaveChanges();
            return StatusCode(201, result);
        }

        [Authorize]
        [HttpGet("my-results")]
        public IActionResult GetMyResults()
        {
            var userId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);

            var results = _context.Results
                .Include(r => r.RaceEntrance)
                    .ThenInclude(e => e!.EventType)
                        .ThenInclude(t => t!.Event)
                .Where(r => r.RaceEntrance != null && r.RaceEntrance.UserId == userId)
                .Select(r => new
                {
                    r.ResultId, r.EntranceId, r.FinishStatus,
                    r.OverallRank, r.CategoryRank, r.AgeGroupRank,
                    EventName = r.RaceEntrance!.EventType!.Event!.EventName,
                    CategoryName = r.RaceEntrance.EventType.EventTypeName
                }).ToList();

            return Ok(results);
        }
    }

    public class ResultDto
    {
        public int EntranceId { get; set; }
        public string FinishStatus { get; set; } = "Finished";
        public int? OverallRank { get; set; }
        public int? CategoryRank { get; set; }
        public int? AgeGroupRank { get; set; }
    }
}
