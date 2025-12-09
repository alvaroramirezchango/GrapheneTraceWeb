using System;
using System.Collections.Generic;
using GrapheneTraceWeb.Models;

namespace GrapheneTraceWeb.ViewModels
{
    public class UserDashboardViewModel
    {
        // Patient user
        public User User { get; set; }

        // Last measurement summary
        public double? LastPeakPressure { get; set; }
        public double? LastContactAreaPercentage { get; set; }
        public DateTime? LastMeasurementTime { get; set; }

        // Recent measurements used in the table and chart
        public List<PressureData> RecentMeasurements { get; set; }
            = new List<PressureData>();

        // Derived alert level based on last peak pressure
        // Possible values: "Critical", "High pressure", "Normal", "Low pressure", "No data"
        public string AlertLevel { get; set; } = "No data";

        // Selected time range for the chart (1h, 6h, 24h, 7d)
        // This allows the controller and view to filter and highlight the chosen range
        public string SelectedRange { get; set; } = "all";
    }
}
