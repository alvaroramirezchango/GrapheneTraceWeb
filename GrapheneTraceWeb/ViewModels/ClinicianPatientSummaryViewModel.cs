using System;

namespace GrapheneTraceWeb.ViewModels
{
    public class ClinicianPatientSummaryViewModel
    {
        public int UserId { get; set; }

        public string Name { get; set; }

        public string Email { get; set; }

        public double? LastPeakPressure { get; set; }

        public double? LastContactArea { get; set; }

        public DateTime? LastMeasurementTime { get; set; }

        // Textual alert level used in clinician dashboard
        public string AlertLevel { get; set; }
    }
}
