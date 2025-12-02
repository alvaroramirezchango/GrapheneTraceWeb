using Microsoft.AspNetCore.Http;
using GrapheneTraceWeb.Data;
using GrapheneTraceWeb.Models;
using GrapheneTraceWeb.Data;
using GrapheneTraceWeb.Models;
using GrapheneTraceWeb.ViewModels;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using System.Diagnostics;
using System.Linq;

namespace GrapheneTraceWeb.Controllers
{
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;
        private readonly AppDbContext _context;

        public HomeController(ILogger<HomeController> logger, AppDbContext context)
        {
            _logger = logger;
            _context = context;
        }

        public IActionResult Index()
        {
            return View();
        }

        [HttpGet]
        public IActionResult Login()
        {
            // Return empty login form
            return View(new LoginViewModel());
        }

        [HttpPost]
        public IActionResult Login(string email, string password, string role)
        {
            // Find user in database
            var user = _context.Users
                .FirstOrDefault(u => u.Email == email && u.Password == password && u.Role == role);

            if (user == null)
            {
                ModelState.AddModelError(string.Empty, "Invalid credentials.");
                return View();
            }

            // Store user in session
            HttpContext.Session.SetInt32(SessionKeys.UserId, user.Id);
            HttpContext.Session.SetString(SessionKeys.UserName, user.Name);
            HttpContext.Session.SetString(SessionKeys.UserRole, user.Role);

            // Redirect based on role
            if (user.Role == "User")
            {
                return RedirectToAction("Dashboard", "User");
            }
            else if (user.Role == "Clinician")
            {
                return RedirectToAction("Dashboard", "Clinician");
            }
            else if (user.Role == "Admin")
            {
                return RedirectToAction("Dashboard", "Admin");
            }

            // Fallback
            return RedirectToAction("Index");
        }
        public IActionResult Logout()
        {
            // Clear all session data for the user
            HttpContext.Session.Clear();
            return RedirectToAction("Login");
        }


        public IActionResult Privacy()
        {
            return View();
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}
