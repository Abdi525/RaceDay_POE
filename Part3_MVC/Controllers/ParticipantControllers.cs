using Microsoft.AspNetCore.Mvc;
using POE_P3_WEB.Models;
using POE_P3_WEB.Services;

namespace POE_P3_WEB.Controllers
{
    public class ParticipantController : Controller
    {
        private readonly ApiClient _apiService;
        private readonly IWebHostEnvironment _env;

        public ParticipantController(ApiClient apiService, IWebHostEnvironment env)
        {
            _apiService = apiService;
            _env = env;
        }

        private bool IsLoggedIn() => !string.IsNullOrWhiteSpace(HttpContext.Session.GetString("UserEmail"));
        private bool IsParticipant() => IsLoggedIn() &&
            string.Equals(HttpContext.Session.GetString("UserRole"), "Participant", StringComparison.OrdinalIgnoreCase);

        public async Task<IActionResult> BrowseEvents()
        {
            if (!IsParticipant()) return RedirectToAction("Login", "Home");
            try
            {
                var events = await _apiService.GetAsync<List<ViewsModels.EventViewModel>>("events");
                return View(events ?? new List<ViewsModels.EventViewModel>());
            }
            catch (HttpRequestException)
            {
                TempData["Error"] = "The API could not be reached.";
                return View(new List<ViewsModels.EventViewModel>());
            }
        }

        [HttpGet]
        public IActionResult Enroll(int eventTypeCode)
        {
            if (!IsParticipant()) return RedirectToAction("Login", "Home");
            return View(new ViewsModels.EnrollmentViewModel { EventTypeCode = eventTypeCode });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Enroll(ViewsModels.EnrollmentViewModel model)
        {
            if (!IsParticipant()) return RedirectToAction("Login", "Home");
            if (!ModelState.IsValid) return View(model);

            try
            {
                using var response = await _apiService.PostAsync("enrolments", model);
                if (response.IsSuccessStatusCode)
                {
                    TempData["Message"] = "Your enrollment was submitted.";
                    return RedirectToAction(nameof(BrowseEvents));
                }
                ModelState.AddModelError(string.Empty, "The API could not complete your enrollment.");
            }
            catch (HttpRequestException)
            {
                ModelState.AddModelError(string.Empty, "Could not connect to the API.");
            }

            return View(model);
        }

        public async Task<IActionResult> MyEvents()
        {
            if (!IsParticipant()) return RedirectToAction("Login", "Home");
            try
            {
                var entries = await _apiService.GetAsync<List<ViewsModels.MyEventViewModel>>("raceentrance/my");
                return View(entries ?? new List<ViewsModels.MyEventViewModel>());
            }
            catch (HttpRequestException)
            {
                TempData["Error"] = "Could not load your event entries.";
                return View(new List<ViewsModels.MyEventViewModel>());
            }
        }

        public async Task<IActionResult> Profile()
        {
            if (!IsParticipant()) return RedirectToAction("Login", "Home");
            try
            {
                var profile = await _apiService.GetAsync<ViewsModels.ProfileViewModel>("users/profile");
                return View(profile ?? new ViewsModels.ProfileViewModel());
            }
            catch (HttpRequestException)
            {
                TempData["Error"] = "Could not load your profile.";
                return View(new ViewsModels.ProfileViewModel());
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Profile(ViewsModels.ProfileViewModel model)
        {
            if (!IsParticipant()) return RedirectToAction("Login", "Home");
            try
            {
                var imagePath = model.ProfileImagePath;
                if (model.ProfileImage is { Length: > 0 })
                {
                    var extension = Path.GetExtension(model.ProfileImage.FileName).ToLowerInvariant();
                    var allowed = new[] { ".jpg", ".jpeg", ".png", ".gif", ".webp" };
                    if (!allowed.Contains(extension))
                    {
                        ModelState.AddModelError(nameof(model.ProfileImage), "Upload a JPG, PNG, GIF or WEBP image.");
                        return View(model);
                    }

                    var folder = Path.Combine(_env.WebRootPath ?? Path.Combine(_env.ContentRootPath, "wwwroot"), "images");
                    Directory.CreateDirectory(folder);
                    var fileName = $"{Guid.NewGuid():N}{extension}";
                    await using (var stream = new FileStream(Path.Combine(folder, fileName), FileMode.CreateNew))
                        await model.ProfileImage.CopyToAsync(stream);
                    imagePath = "/images/" + fileName;
                }

                using var response = await _apiService.PutAsync("users/profile", new { ProfileImagePath = imagePath });
                if (response.IsSuccessStatusCode)
                {
                    TempData["Message"] = "Profile updated.";
                    return RedirectToAction(nameof(Profile));
                }
                ModelState.AddModelError(string.Empty, "The API could not update your profile.");
            }
            catch (HttpRequestException)
            {
                ModelState.AddModelError(string.Empty, "Could not connect to the API.");
            }

            return View(model);
        }

        public async Task<IActionResult> MyResults()
        {
            if (!IsParticipant()) return RedirectToAction("Login", "Home");
            try
            {
                var results = await _apiService.GetAsync<List<ViewsModels.ResultViewModel>>("results/my-results");
                return View(results ?? new List<ViewsModels.ResultViewModel>());
            }
            catch (HttpRequestException)
            {
                TempData["Error"] = "The API could not be reached.";
                return View(new List<ViewsModels.ResultViewModel>());
            }
        }
    }
}
