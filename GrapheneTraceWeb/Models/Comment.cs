using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace GrapheneTraceWeb.Models
{
    public class Comment
    {
        public int Id { get; set; }

        // Foreign key: the measurement this comment is about
        public int PressureDataId { get; set; }

        [ForeignKey("PressureDataId")]
        public PressureData? PressureData { get; set; }

        // Patient (user) who wrote the comment
        public int UserId { get; set; }

        [ForeignKey("UserId")]
        public User? User { get; set; }

        // Optional clinician replying to the comment
        public int? ClinicianId { get; set; }

        [ForeignKey("ClinicianId")]
        public User? Clinician { get; set; }

        // Optional parent comment (for threaded replies)
        public int? ParentCommentId { get; set; }

        [ForeignKey("ParentCommentId")]
        public Comment? ParentComment { get; set; }

        // Actual comment text - this is the ONLY required field
        [Required]
        [StringLength(500)]
        public string Text { get; set; } = string.Empty;

        // When the comment was created
        public DateTime Timestamp { get; set; } = DateTime.Now;
    }
}
