using Microsoft.AspNetCore.Mvc;

namespace GrapheneTraceWeb.Controllers
{
    public class HomeController : Controller
    {
        public IActionResult Index()
        {
            return View();
        }

        public IActionResult Login()
        {
            return View();
        }

        [HttpPost]
        public IActionResult Login(string email, string password, string role)
        {
            // For now, ignore validation until we connect database
            if (role == "Clinician")
                return RedirectToAction("Dashboard", "Clinician");

            if (role == "Admin")
                return RedirectToAction("Dashboard", "Admin");

            // Default: user
            return RedirectToAction("Dashboard", "User");
        }  //Prueba con commit
    }
}
