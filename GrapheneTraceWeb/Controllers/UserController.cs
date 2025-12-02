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
            var sessionUserId = HttpContext.Session.GetInt32(SessionKeys.UserId);
            var role = HttpContext.Session.GetString(SessionKeys.UserRole);

            if (sessionUserId == null || string.IsNullOrEmpty(role))
            {
                return RedirectToAction("Login", "Home");
            }

            int targetUserId;

            // Case 1: normal user -> always see their own dashboard
            if (role == "User")
            {
                targetUserId = sessionUserId.Value;
            }
            // Case 2: clinician or admin -> needs a patient id in the query string
            else if ((role == "Clinician" || role == "Admin") && id.HasValue)
            {
                targetUserId = id.Value;
            }
            else
            {
                // No permission or missing patient id
                return RedirectToAction("Login", "Home");
            }

            // Make sure the target user exists and is a "User" (patient)
            var user = _context.Users
                .FirstOrDefault(u => u.Id == targetUserId && u.Role == "User");

            if (user == null)
            {
                return NotFound();
            }

            // Load recent pressure data for this user
            var recentMeasurements = _context.PressureData
                .Where(p => p.UserId == targetUserId)
                .OrderByDescending(p => p.Timestamp)
                .Take(10)
                .ToList();

            var lastMeasurement = recentMeasurements.FirstOrDefault();

            var viewModel = new UserDashboardViewModel
            {
                User = user,
                LastPeakPressure = lastMeasurement?.PeakPressure,
                LastContactAreaPercentage = lastMeasurement?.ContactArea,
                LastMeasurementTime = lastMeasurement?.Timestamp,
                RecentMeasurements = recentMeasurements
            };

            return View(viewModel);
        }


    }
}
