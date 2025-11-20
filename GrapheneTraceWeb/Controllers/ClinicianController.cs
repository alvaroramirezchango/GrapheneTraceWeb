using Microsoft.AspNetCore.Mvc;

namespace GrapheneTraceWeb.Controllers
{
    public class ClinicianController : Controller
    {
        public IActionResult Dashboard()
        {
            return View();
        }
    }
}
