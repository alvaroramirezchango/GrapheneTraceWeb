using System;
using System.Collections.Generic;

namespace GrapheneTraceWeb.ViewModels
{
    public class ClinicianPatientSummaryViewModel
    {
        public int UserId { get; set; }
        public string Name { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public double? LastPeakPressure { get; set; }
        public double? LastContactArea { get; set; }
        public DateTime? LastMeasurementTime { get; set; }
        public string AlertLevel { get; set; } = string.Empty;
    }

    public class ClinicianDashboardViewModel
    {
        public List<ClinicianPatientSummaryViewModel> Patients { get; set; }
            = new List<ClinicianPatientSummaryViewModel>();
    }
}
