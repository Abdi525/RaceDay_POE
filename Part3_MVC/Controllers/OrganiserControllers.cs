using Microsoft.AspNetCore.Mvc;
using POE_P3_WEB.Models;
using POE_P3_WEB.Services;

namespace POE_P3_WEB.Controllers
{
    public class OrganiserController : Controller
    {
        private readonly ApiClient _apiService;
        private readonly IWebHostEnvironment _env;

        public OrganiserController(ApiClient apiService, IWebHostEnvironment env)
        {
            _apiService = apiService;
            _env = env;
        }

        private bool IsOrganiser() =>
            string.Equals(HttpContext.Session.GetString("UserRole"), "Organiser", StringComparison.OrdinalIgnoreCase);

        public async Task<IActionResult> Dashboard()
        {
            if (!IsOrganiser()) return RedirectToAction("Login", "Home");

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
        public IActionResult CreateEvent()
        {
            if (!IsOrganiser()) return RedirectToAction("Login", "Home");
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CreateEvent(ViewsModels.EventViewModel model)
        {
            if (!IsOrganiser()) return RedirectToAction("Login", "Home");
            if (!ModelState.IsValid) return View(model);

            string? savedImagePath = null;
            if (model.BannerImage is { Length: > 0 })
            {
                var extension = Path.GetExtension(model.BannerImage.FileName).ToLowerInvariant();
                var allowedExtensions = new[] { ".jpg", ".jpeg", ".png", ".gif", ".webp" };
                if (!allowedExtensions.Contains(extension))
                {
                    ModelState.AddModelError(nameof(model.BannerImage), "Upload a JPG, PNG, GIF or WEBP image.");
                    return View(model);
                }

                var uploadsFolder = Path.Combine(_env.WebRootPath ?? Path.Combine(_env.ContentRootPath, "wwwroot"), "images");
                Directory.CreateDirectory(uploadsFolder);
                var uniqueFileName = $"{Guid.NewGuid():N}{extension}";
                var filePath = Path.Combine(uploadsFolder, uniqueFileName);

                await using (var stream = new FileStream(filePath, FileMode.CreateNew))
                    await model.BannerImage.CopyToAsync(stream);

                savedImagePath = "/images/" + uniqueFileName;
            }

            try
            {
                // Do not send IFormFile to the API. The API receives only the event data and local image URL.
                var request = new
                {
                    model.EventName,
                    model.EventLocation,
                    model.EventDate,
                    model.EventDescription,
                    ImagePath = savedImagePath
                };

                using var response = await _apiService.PostAsync("events", request);
                if (response.IsSuccessStatusCode)
                    return RedirectToAction(nameof(Dashboard));

                if (savedImagePath != null)
                    TryDeleteLocalImage(savedImagePath);

                ModelState.AddModelError(string.Empty, "The API could not create the event. Check the API response and event fields.");
            }
            catch (HttpRequestException)
            {
                if (savedImagePath != null) TryDeleteLocalImage(savedImagePath);
                ModelState.AddModelError(string.Empty, "Could not connect to the API. Check that it is running.");
            }

            return View(model);
        }

        [HttpGet]
        public IActionResult AddCategory(int eventId)
        {
            if (!IsOrganiser()) return RedirectToAction("Login", "Home");
            return View(new ViewsModels.EventTypeViewModel { EventId = eventId });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> AddCategory(ViewsModels.EventTypeViewModel model)
        {
            if (!IsOrganiser()) return RedirectToAction("Login", "Home");
            if (!ModelState.IsValid) return View(model);

            try
            {
                using var response = await _apiService.PostAsync($"events/{model.EventId}/categories", new { model.EventTypeName, model.DistanceKM, model.EntranceFee, model.MaxParticipants });
                if (response.IsSuccessStatusCode) return RedirectToAction(nameof(Dashboard));
                ModelState.AddModelError(string.Empty, "The API could not save this category.");
            }
            catch (HttpRequestException)
            {
                ModelState.AddModelError(string.Empty, "Could not connect to the API.");
            }

            return View(model);
        }

        [HttpGet]
        public async Task<IActionResult> EditEvent(int id)
        {
            if (!IsOrganiser()) return RedirectToAction("Login", "Home");
            try
            {
                var model = await _apiService.GetAsync<ViewsModels.EventViewModel>($"events/{id}");
                if (model == null) return NotFound();
                return View(model);
            }
            catch (HttpRequestException)
            {
                TempData["Error"] = "Could not load the event from the API.";
                return RedirectToAction(nameof(Dashboard));
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> EditEvent(ViewsModels.EventViewModel model)
        {
            if (!IsOrganiser()) return RedirectToAction("Login", "Home");
            if (!ModelState.IsValid) return View(model);

            string? newImagePath = null;
            if (model.BannerImage is { Length: > 0 })
            {
                newImagePath = await SaveLocalImageAsync(model.BannerImage);
                if (newImagePath == null)
                {
                    ModelState.AddModelError(nameof(model.BannerImage), "Upload a JPG, PNG, GIF or WEBP image.");
                    return View(model);
                }
            }

            try
            {
                var request = new
                {
                    model.EventName, model.EventLocation, model.EventDate,
                    model.EventDescription,
                    ImagePath = newImagePath ?? model.ImagePath
                };
                using var response = await _apiService.PutAsync($"events/{model.EventId}", request);
                if (response.IsSuccessStatusCode) return RedirectToAction(nameof(Dashboard));
                if (newImagePath != null) TryDeleteLocalImage(newImagePath);
                ModelState.AddModelError(string.Empty, "The API could not update this event.");
            }
            catch (HttpRequestException)
            {
                if (newImagePath != null) TryDeleteLocalImage(newImagePath);
                ModelState.AddModelError(string.Empty, "Could not connect to the API.");
            }
            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteEvent(int eventId)
        {
            if (!IsOrganiser()) return RedirectToAction("Login", "Home");
            try
            {
                using var response = await _apiService.DeleteAsync($"events/{eventId}");
                if (!response.IsSuccessStatusCode)
                    TempData["Error"] = "The API could not delete this event. It may have existing enrolments.";
            }
            catch (HttpRequestException)
            {
                TempData["Error"] = "Could not connect to the API.";
            }
            return RedirectToAction(nameof(Dashboard));
        }

        public async Task<IActionResult> Enrollments(int eventId)
        {
            if (!IsOrganiser()) return RedirectToAction("Login", "Home");
            try
            {
                var entries = await _apiService.GetAsync<List<ViewsModels.EnrollmentAdminViewModel>>($"events/{eventId}/enrolments");
                ViewBag.EventId = eventId;
                return View(entries ?? new List<ViewsModels.EnrollmentAdminViewModel>());
            }
            catch (HttpRequestException)
            {
                TempData["Error"] = "Could not load participant enrolments.";
                return RedirectToAction(nameof(Dashboard));
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> RecordResult(ViewsModels.ResultCaptureViewModel model)
        {
            if (!IsOrganiser()) return RedirectToAction("Login", "Home");
            try
            {
                using var response = await _apiService.PostAsync("results", new
                {
                    model.EntranceId, model.FinishStatus, model.OverallRank,
                    model.CategoryRank, model.AgeGroupRank
                });
                if (response.IsSuccessStatusCode)
                    TempData["Message"] = "Race result recorded successfully.";
                else
                    TempData["Error"] = "The API could not record this result. Check whether a result already exists.";
            }
            catch (HttpRequestException)
            {
                TempData["Error"] = "Could not connect to the API.";
            }
            return RedirectToAction(nameof(Enrollments), new { eventId = model.EventId });
        }

        private async Task<string?> SaveLocalImageAsync(IFormFile image)
        {
            var extension = Path.GetExtension(image.FileName).ToLowerInvariant();
            var allowedExtensions = new[] { ".jpg", ".jpeg", ".png", ".gif", ".webp" };
            if (!allowedExtensions.Contains(extension)) return null;

            var uploadsFolder = Path.Combine(_env.WebRootPath ?? Path.Combine(_env.ContentRootPath, "wwwroot"), "images");
            Directory.CreateDirectory(uploadsFolder);
            var fileName = $"{Guid.NewGuid():N}{extension}";
            var fullPath = Path.Combine(uploadsFolder, fileName);
            await using (var stream = new FileStream(fullPath, FileMode.CreateNew))
                await image.CopyToAsync(stream);
            return "/images/" + fileName;
        }

        private void TryDeleteLocalImage(string imagePath)
        {
            var relativePath = imagePath.TrimStart('/').Replace('/', Path.DirectorySeparatorChar);
            var fullPath = Path.Combine(_env.WebRootPath ?? Path.Combine(_env.ContentRootPath, "wwwroot"), relativePath);
            if (System.IO.File.Exists(fullPath)) System.IO.File.Delete(fullPath);
        }
    }
}
