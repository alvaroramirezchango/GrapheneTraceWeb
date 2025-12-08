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

            // Ensure the target user exists and is a patient
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

            // Compute alert level based on last peak pressure
            var alertLevel = GetAlertLevel(lastMeasurement?.PeakPressure);

            var viewModel = new UserDashboardViewModel
            {
                User = user,
                LastPeakPressure = lastMeasurement?.PeakPressure,
                LastContactAreaPercentage = lastMeasurement?.ContactArea,
                LastMeasurementTime = lastMeasurement?.Timestamp,
                RecentMeasurements = recentMeasurements,
                AlertLevel = alertLevel
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
