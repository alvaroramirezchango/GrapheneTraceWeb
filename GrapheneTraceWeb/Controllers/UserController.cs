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

        public IActionResult Dashboard(int? id)
        {
            var user = id.HasValue
                ? _context.Users.FirstOrDefault(u => u.Id == id.Value)
                : _context.Users.FirstOrDefault(u => u.Role == "User");

            if (user == null)
            {
                return NotFound();
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
                LastContactAreaPercentage = last?.ContactArea,
                LastMeasurementTime = last?.Timestamp,
                RecentMeasurements = measurements
            };

            return View(vm);
        }

    }
}
