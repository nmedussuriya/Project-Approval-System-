using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace PAS_Full_System.Models
{
    public class MeetingBooking
    {
        public int Id { get; set; }

        [Required]
        public int AvailabilityId { get; set; }

        [ForeignKey("AvailabilityId")]
        public SupervisorAvailability? Availability { get; set; }

        [Required]
        public string StudentId { get; set; } = string.Empty;

        [ForeignKey("StudentId")]
        public ApplicationUser? Student { get; set; }

        [Required]
        public string SupervisorId { get; set; } = string.Empty;

        [ForeignKey("SupervisorId")]
        public ApplicationUser? Supervisor { get; set; }

        [Required]
        public int ProjectId { get; set; }

        [ForeignKey("ProjectId")]
        public Project? Project { get; set; }

        [Required]
        public string Status { get; set; } = "Approved";
    }
}