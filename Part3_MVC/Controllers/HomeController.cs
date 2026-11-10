using System.Text.Json;
using Microsoft.AspNetCore.Mvc;
using POE_P3_WEB.Models;
using POE_P3_WEB.Services;

namespace POE_P3_WEB.Controllers
{
    public class HomeController : Controller
    {
        private readonly ApiClient _apiService;

        public HomeController(ApiClient apiService) => _apiService = apiService;

        [HttpGet]
        public IActionResult Index() => View();

        [HttpGet]
        public IActionResult Login() => View("~/Views/Auth/Login.cshtml");

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Login(ViewsModels.LoginViewModel model)
        {
            if (!ModelState.IsValid) return View("~/Views/Auth/Login.cshtml", model);

            try
            {
                using var response = await _apiService.PostAsync("auth/login", model);
                if (!response.IsSuccessStatusCode)
                {
                    ModelState.AddModelError(string.Empty, "Invalid email or password.");
                    return View("~/Views/Auth/Login.cshtml", model);
                }

                var body = await response.Content.ReadAsStringAsync();
                string role = "Participant";
                string? token = null;

                if (!string.IsNullOrWhiteSpace(body))
                {
                    using var document = JsonDocument.Parse(body);
                    role = ApiClient.ReadStringProperty(document.RootElement, "userRole", "role", "roleName") ?? role;
                    token = ApiClient.ReadStringProperty(document.RootElement, "token", "accessToken");
                }

                HttpContext.Session.SetString("UserEmail", model.Email);
                HttpContext.Session.SetString("UserRole", role);
                if (!string.IsNullOrWhiteSpace(token))
                    HttpContext.Session.SetString("ApiToken", token);
                else
                {
                    ModelState.AddModelError(string.Empty, "The API did not return an authentication token.");
                    return View("~/Views/Auth/Login.cshtml", model);
                }

                return string.Equals(role, "Organiser", StringComparison.OrdinalIgnoreCase)
                    ? RedirectToAction("Dashboard", "Organiser")
                    : RedirectToAction("BrowseEvents", "Participant");
            }
            catch (HttpRequestException)
            {
                ModelState.AddModelError(string.Empty, "Could not connect to the API. Check that your API is running and that ApiSettings:BaseUrl is correct.");
                return View("~/Views/Auth/Login.cshtml", model);
            }
            catch (JsonException)
            {
                ModelState.AddModelError(string.Empty, "The API returned an invalid login response.");
                return View("~/Views/Auth/Login.cshtml", model);
            }
        }

        [HttpGet]
        public IActionResult Register() => View("~/Views/Auth/Register.cshtml");

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Register(ViewsModels.RegisterViewModel model)
        {
            if (!ModelState.IsValid) return View("~/Views/Auth/Register.cshtml", model);

            try
            {
                using var response = await _apiService.PostAsync("auth/register", model);
                if (response.IsSuccessStatusCode)
                {
                    TempData["Message"] = "Registration successful. Please log in.";
                    return RedirectToAction(nameof(Login));
                }

                ModelState.AddModelError(string.Empty, "Registration failed. Check the details and try again.");
            }
            catch (HttpRequestException)
            {
                ModelState.AddModelError(string.Empty, "Could not connect to the API. Check that your API is running.");
            }

            return View("~/Views/Auth/Register.cshtml", model);
        }

        public IActionResult Logout()
        {
            HttpContext.Session.Clear();
            return RedirectToAction(nameof(Login));
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error() => View("~/Views/Shared/Error.cshtml",
            new ErrorViewModel { RequestId = HttpContext.TraceIdentifier });
    }
}
