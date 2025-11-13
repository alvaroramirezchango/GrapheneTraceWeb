using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace GrapheneTraceWeb.Models
{
    public class PressureData
    {
        public int Id { get; set; }

        // Link to User (Patient)
        [Required]
        public int UserId { get; set; }

        [ForeignKey("UserId")]
        public User User { get; set; }

        // When the measurement was taken
        public DateTime Timestamp { get; set; } = DateTime.Now;

        // Key metrics
        public double PeakPressure { get; set; }
        public double ContactArea { get; set; }

        // Optional: raw heatmap data (CSV string or JSON matrix)
        public string DataMatrix { get; set; }
    }
}
