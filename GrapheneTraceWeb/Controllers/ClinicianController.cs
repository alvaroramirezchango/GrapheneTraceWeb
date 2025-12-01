using GrapheneTraceWeb.Data;
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

        public IActionResult Dashboard()
        {
            // Get all users that are patients (role "User")
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
    }
}
