using Microsoft.AspNetCore.Http;
using GrapheneTraceWeb;
using GrapheneTraceWeb.Data;
using GrapheneTraceWeb.Models;
using GrapheneTraceWeb.ViewModels;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Linq;
using System.Collections.Generic;

namespace GrapheneTraceWeb.Controllers
{
    public class ClinicianController : Controller
    {
        private readonly AppDbContext _context;

        public ClinicianController(AppDbContext context)
        {
            _context = context;
        }

        // =========================
        // DASHBOARD
        // =========================
        public IActionResult Dashboard()
        {
            var userId = HttpContext.Session.GetInt32(SessionKeys.UserId);
            var role = HttpContext.Session.GetString(SessionKeys.UserRole);

            // Allow both Clinician and Admin to access this dashboard
            if (userId == null || (role != "Clinician" && role != "Admin"))
            {
                return RedirectToAction("Login", "Home");
            }

            // Get all users that are patients
            var users = _context.Users
                .Where(u => u.Role == "User")
                .ToList();

            var summaries = new List<ClinicianPatientSummaryViewModel>();

            foreach (var user in users)
            {
                var lastMeasurement = _context.PressureData
                    .Where(p => p.UserId == user.Id)
                    .OrderByDescending(p => p.Timestamp)
                    .FirstOrDefault();

                double? peak = lastMeasurement?.PeakPressure;
                double? contact = lastMeasurement?.ContactArea;
                DateTime? time = lastMeasurement?.Timestamp;

                var summary = new ClinicianPatientSummaryViewModel
                {
                    UserId = user.Id,
                    Name = user.Name,
                    Email = user.Email,
                    LastPeakPressure = peak,
                    LastContactArea = contact,
                    LastMeasurementTime = time,
                    AlertLevel = GetAlertLevel(peak)
                };

                summaries.Add(summary);
            }

            var orderedSummaries = summaries
                .OrderByDescending(s => AlertSeverityScore(s.AlertLevel))
                .ThenByDescending(s => s.LastMeasurementTime)
                .ToList();

            var vm = new ClinicianDashboardViewModel
            {
                Patients = orderedSummaries
            };

            return View(vm);
        }

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

        private int AlertSeverityScore(string alert)
        {
            return alert switch
            {
                "Critical" => 3,
                "High pressure" => 2,
                "Normal" => 1,
                "Low pressure" => 0,
                _ => -1
            };
        }

        // ========================================
        // COMMENT REPLY (GET)
        // ========================================
        [HttpGet]
        public IActionResult ReplyComment(int id)
        {
            var sessionUserId = HttpContext.Session.GetInt32(SessionKeys.UserId);
            var role = HttpContext.Session.GetString(SessionKeys.UserRole);

            if (sessionUserId == null || (role != "Clinician" && role != "Admin"))
            {
                return RedirectToAction("Login", "Home");
            }

            var parent = _context.Comments.FirstOrDefault(c => c.Id == id);

            if (parent == null)
            {
                return NotFound();
            }

            var replyModel = new Comment
            {
                ParentCommentId = parent.Id,
                PressureDataId = parent.PressureDataId,
                UserId = parent.UserId,            // patient
                ClinicianId = sessionUserId.Value  // clinician replying
            };

            ViewBag.ParentComment = parent;
            return View(replyModel);
        }

        // ========================================
        // COMMENT REPLY (POST)
        // ========================================
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult ReplyComment(Comment model)
        {
            var sessionUserId = HttpContext.Session.GetInt32(SessionKeys.UserId);
            var role = HttpContext.Session.GetString(SessionKeys.UserRole);

            // Only clinicians and admins can reply
            if (sessionUserId == null || (role != "Clinician" && role != "Admin"))
            {
                return RedirectToAction("Login", "Home");
            }

            // Remove validation for navigation properties (not provided in the form)
            ModelState.Remove("PressureData");
            ModelState.Remove("User");
            ModelState.Remove("Clinician");
            ModelState.Remove("ParentComment");

            if (model.ParentCommentId == null)
            {
                return BadRequest();
            }

            // Load the original patient comment
            var parent = _context.Comments.FirstOrDefault(c => c.Id == model.ParentCommentId.Value);

            if (parent == null)
            {
                return NotFound();
            }

            // Validate reply text
            if (string.IsNullOrWhiteSpace(model.Text))
            {
                ModelState.AddModelError("Text", "Reply text is required.");
            }

            if (!ModelState.IsValid)
            {
                // Rebuild view context in case of invalid data
                ViewBag.ParentComment = parent;
                return View(model);
            }

            // IMPORTANT:
            // Create a new Comment entity for the reply instead of modifying the posted model.
            // This avoids EF Core tracking conflicts.
            var reply = new Comment
            {
                Text = model.Text,
                ParentCommentId = parent.Id,              // link reply to the original comment
                PressureDataId = parent.PressureDataId,   // link reply to the same measurement
                UserId = parent.UserId,                   // patient ID
                ClinicianId = sessionUserId.Value,        // clinician writing the reply
                Timestamp = DateTime.Now
            };

            // Save reply
            _context.Comments.Add(reply);
            _context.SaveChanges();

            TempData["ReplySaved"] = "Reply added successfully.";

            return RedirectToAction("Dashboard");
        }


    }
}
