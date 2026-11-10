using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using poe11.RaceDayDatabase;

namespace poe11.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class UserController : ControllerBase
    {
        private readonly RaceDayDatabaseContext _context;
        public UserController(RaceDayDatabaseContext context) => _context = context;

        [Authorize]
        [HttpGet("profile")]
        [HttpGet("/api/users/profile")]
        public IActionResult GetProfile()
        {
            var userId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
            var user = _context.Users.Find(userId);
            if (user == null) return NotFound();

            return Ok(new
            {
                user.UserId, user.FirstName, user.LastName, user.Email,
                user.PhoneNumber, user.Address, user.UserRole, user.ProfileImagePath
            });
        }

        [Authorize]
        [HttpPut("profile")]
        [HttpPut("/api/users/profile")]
        public IActionResult UpdateProfile([FromBody] UpdateProfileDto dto)
        {
            var userId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
            var user = _context.Users.Find(userId);
            if (user == null) return NotFound();

            user.ProfileImagePath = dto.ProfileImagePath;
            _context.SaveChanges();
            return Ok(new { user.UserId, user.ProfileImagePath });
        }
    }

    public class UpdateProfileDto
    {
        public string? ProfileImagePath { get; set; }
    }
}
