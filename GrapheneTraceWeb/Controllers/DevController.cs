using System;
using System.Linq;
using GrapheneTraceWeb.Data;
using GrapheneTraceWeb.Models;
using Microsoft.AspNetCore.Mvc;

namespace GrapheneTraceWeb.Controllers
{
    /// <summary>
    /// Development-only controller used to seed fake demo data.
    /// Remove this controller for production if needed.
    /// </summary>
    public class DevController : Controller
    {
        private readonly AppDbContext _context;

        public DevController(AppDbContext context)
        {
            _context = context;
        }

        /// <summary>
        /// Seeds fake pressure data for all users with Role = "User"
        /// for the last 7 days (one measurement every hour).
        /// Call this action once by visiting /Dev/SeedDemoPressureData.
        /// </summary>
        public IActionResult SeedDemoPressureData()
        {
            // Delete all existing pressure data
            _context.PressureData.RemoveRange(_context.PressureData);
            _context.SaveChanges();

            var random = new Random();

            // Get all patient users
            var users = _context.Users
                .Where(u => u.Role == "User")
                .ToList();

            if (!users.Any())
            {
                return Content("No users with Role = 'User' were found.");
            }

            DateTime now = DateTime.Now;

            foreach (var user in users)
            {
                // Create measurements for the last 7 days, one per hour
                for (int hoursBack = 0; hoursBack <= 7 * 24; hoursBack++)
                {
                    var timestamp = now.AddHours(-hoursBack);

                    // Generate plausible fake values
                    double peak = 90 + random.Next(0, 51);     // 90–140 mmHg
                    double contact = 60 + random.Next(0, 26);  // 60–85 %

                    var measurement = new PressureData
                    {
                        UserId = user.Id,
                        Timestamp = timestamp,
                        PeakPressure = peak,
                        ContactArea = contact,
                        // Provide a non-null value for DataMatrix to satisfy NOT NULL constraint
                        DataMatrix = "Demo matrix data"
                    };

                    _context.PressureData.Add(measurement);
                }
            }

            _context.SaveChanges();

            return Content("Demo pressure data seeded successfully (existing data was cleared).");
        }


    }
}
