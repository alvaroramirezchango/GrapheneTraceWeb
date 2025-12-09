using System;
using System.Linq;
using GrapheneTraceWeb.Data;
using GrapheneTraceWeb.Models;
using GrapheneTraceWeb.ViewModels;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace GrapheneTraceWeb.Controllers
{
    public class UserController : Controller
    {
        private readonly AppDbContext _context;

        public UserController(AppDbContext context)
        {
            _context = context;
        }

        // =========================
        // User Dashboard
        // =========================
        public IActionResult Dashboard(int? id, string timeRange = "all")
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

            // Ensure the target user exists and is a patient
            var user = _context.Users
                .FirstOrDefault(u => u.Id == targetUserId && u.Role == "User");

            if (user == null)
            {
                return NotFound();
            }

            // Calculate starting date based on selected time range
            DateTime? fromDate = GetFromDate(timeRange);

            // Load pressure data for this user
            var measurementsQuery = _context.PressureData
                .Where(p => p.UserId == targetUserId);

            if (fromDate.HasValue)
            {
                // Filter measurements by selected time range
                measurementsQuery = measurementsQuery
                    .Where(p => p.Timestamp >= fromDate.Value);
            }

            // Order ascending for the chart
            var recentMeasurements = measurementsQuery
                .OrderBy(p => p.Timestamp)
                .ToList();

            // Last measurement is the most recent one
            var lastMeasurement = recentMeasurements.LastOrDefault();

            // Compute alert level based on last peak pressure
            var alertLevel = GetAlertLevel(lastMeasurement?.PeakPressure);

            var viewModel = new UserDashboardViewModel
            {
                User = user,
                LastPeakPressure = lastMeasurement?.PeakPressure,
                LastContactAreaPercentage = lastMeasurement?.ContactArea,
                LastMeasurementTime = lastMeasurement?.Timestamp,
                RecentMeasurements = recentMeasurements,
                AlertLevel = alertLevel,
                // Store selected time range so the view can highlight the active button
                SelectedRange = timeRange
            };

            return View(viewModel);
        }

        // =========================
        // Helper: alert level logic
        // =========================
        /// <summary>
        /// Returns a human-readable alert level for the given peak pressure.
        /// </summary>
        private string GetAlertLevel(double? peak)
        {
            if (!peak.HasValue)
                return "No data";

            if (peak.Value >= 130)
                return "Critical";

            if (peak.Value >= 120)
                return "High pressure";

            if (peak.Value >= 90)
                return "Normal";

            return "Low pressure";
        }

        // =========================
        // Helper: time range logic
        // =========================
        /// <summary>
        /// Returns the starting DateTime for the given time range.
        /// "all" or unknown values return null (no time filter).
        /// </summary>
        private DateTime? GetFromDate(string timeRange)
        {
            if (string.IsNullOrWhiteSpace(timeRange) || timeRange == "all")
            {
                // No filter by time
                return null;
            }

            DateTime now = DateTime.Now;

            switch (timeRange)
            {
                case "1h":
                    return now.AddHours(-1);
                case "6h":
                    return now.AddHours(-6);
                case "24h":
                    return now.AddHours(-24);
                case "7d":
                    return now.AddDays(-7);
                default:
                    // Unknown -> do not filter
                    return null;
            }
        }

        // =========================
        // Add Comment (GET)
        // =========================
        [HttpGet]
        public IActionResult AddComment(int pressureDataId)
        {
            var sessionUserId = HttpContext.Session.GetInt32(SessionKeys.UserId);
            var role = HttpContext.Session.GetString(SessionKeys.UserRole);

            // Only logged-in normal users can add comments in this prototype
            if (sessionUserId == null || role != "User")
            {
                return RedirectToAction("Login", "Home");
            }

            // Ensure the measurement exists and belongs to the logged-in user
            var measurement = _context.PressureData
                .FirstOrDefault(p => p.Id == pressureDataId && p.UserId == sessionUserId.Value);

            if (measurement == null)
            {
                return NotFound();
            }

            var model = new Comment
            {
                PressureDataId = measurement.Id
                // UserId will be set on POST
            };

            return View(model);
        }

        // =========================
        // Add Comment (POST)
        // =========================
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult AddComment(Comment model)
        {
            var sessionUserId = HttpContext.Session.GetInt32(SessionKeys.UserId);
            var role = HttpContext.Session.GetString(SessionKeys.UserRole);

            if (sessionUserId == null || role != "User")
            {
                return RedirectToAction("Login", "Home");
            }

            // Ignore validation for navigation properties
            ModelState.Remove("PressureData");
            ModelState.Remove("User");
            ModelState.Remove("Clinician");
            ModelState.Remove("ParentComment");

            // Validate text field
            if (string.IsNullOrWhiteSpace(model.Text))
            {
                ModelState.AddModelError("Text", "Comment text is required.");
            }

            // Confirm the measurement exists and belongs to this user
            var measurement = _context.PressureData
                .FirstOrDefault(p => p.Id == model.PressureDataId && p.UserId == sessionUserId.Value);

            if (measurement == null)
            {
                return NotFound();
            }

            if (!ModelState.IsValid)
            {
                return View(model);
            }

            // Set fields that are not posted from the form
            model.UserId = sessionUserId.Value;
            model.Timestamp = DateTime.Now;

            _context.Comments.Add(model);
            _context.SaveChanges();

            TempData["CommentSaved"] = "Your comment has been saved successfully.";

            // User always goes back to their own dashboard
            return RedirectToAction("Dashboard");
        }
    }
}
