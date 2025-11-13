using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace GrapheneTraceWeb.Models
{
    public class Comment
    {
        public int Id { get; set; }

        // The patient who wrote the comment OR the patient the comment belongs to
        [Required]
        public int UserId { get; set; }

        [ForeignKey("UserId")]
        public User User { get; set; }

        // Clinician responding (nullable because patient may comment first)
        public int? ClinicianId { get; set; }

        [ForeignKey("ClinicianId")]
        public User Clinician { get; set; }

        // The actual text message
        [Required]
        public string Text { get; set; }

        // When the comment was written
        public DateTime Timestamp { get; set; } = DateTime.Now;
    }
}
