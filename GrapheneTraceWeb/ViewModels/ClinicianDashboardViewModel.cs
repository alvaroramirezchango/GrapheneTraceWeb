using System.Collections.Generic;

namespace GrapheneTraceWeb.ViewModels
{
    public class ClinicianDashboardViewModel
    {
        // List of patient summaries visible to the clinician
        public List<ClinicianPatientSummaryViewModel> Patients { get; set; }
            = new List<ClinicianPatientSummaryViewModel>();
    }
}
