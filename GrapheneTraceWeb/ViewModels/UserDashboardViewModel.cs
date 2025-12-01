using System;
using System.Collections.Generic;
using GrapheneTraceWeb.Models;

namespace GrapheneTraceWeb.ViewModels
{
    public class UserDashboardViewModel
    {
        public User User { get; set; }

        // Última medición
        public double? LastPeakPressure { get; set; }
        public double? LastContactAreaPercentage { get; set; }
        public DateTime? LastMeasurementTime { get; set; }

        // Historial reciente
        public List<PressureData> RecentMeasurements { get; set; } = new();
    }
}
