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
        public IActionResult Login(LoginViewModel model)
        {
            if (!ModelState.IsValid)
            {
                // Validation errors in the form
                return View(model);
            }

            // Look for user by email
            var user = _context.Users.FirstOrDefault(u => u.Email == model.Email);

            if (user == null)
            {
                ModelState.AddModelError(string.Empty, "User not found.");
                return View(model);
            }

            // Simple password check (for academic purposes, no hashing)
            if (user.Password != model.Password)
            {
                ModelState.AddModelError(string.Empty, "Incorrect password.");
                return View(model);
            }

            // Redirect based on user role
            if (user.Role == "User")
            {
                // Redirect to that specific user's dashboard
                return RedirectToAction("Dashboard", "User", new { id = user.Id });
            }
            else if (user.Role == "Clinician")
            {
                return RedirectToAction("Dashboard", "Clinician");
            }
            else if (user.Role == "Admin")
            {
                
                return RedirectToAction("Dashboard", "Admin");
            }

            // Fallback: go to home if role is unknown
            return RedirectToAction("Index");
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
