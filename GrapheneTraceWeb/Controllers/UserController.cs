using GrapheneTraceWeb.Data;
using GrapheneTraceWeb.ViewModels;
using Microsoft.AspNetCore.Mvc;
using System.Linq;

namespace GrapheneTraceWeb.Controllers
{
    public class UserController : Controller
    {
        private readonly AppDbContext _context;

        public UserController(AppDbContext context)
        {
            _context = context;
        }

        public IActionResult Dashboard()
        {
            // De momento usamos siempre al primer usuario con rol "User" (Alice)
            var user = _context.Users.FirstOrDefault(u => u.Role == "User");

            if (user == null)
            {
                // Si no hay usuarios, devolvemos un modelo vacío
                return View(new UserDashboardViewModel());
            }

            var measurements = _context.PressureData
                .Where(p => p.UserId == user.Id)
                .OrderByDescending(p => p.Timestamp)
                .ToList();

            var last = measurements.FirstOrDefault();

            var vm = new UserDashboardViewModel
            {
                User = user,
                LastPeakPressure = last?.PeakPressure,
                // Aquí usamos ContactArea (como se llama en tu modelo)
                LastContactAreaPercentage = last?.ContactArea,
                LastMeasurementTime = last?.Timestamp,
                RecentMeasurements = measurements
            };

            return View(vm);
        }
    }
}
