using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace PAS_Full_System.Models
{
    public class SupervisorAvailability
    {
        public int Id { get; set; }

        public string? SupervisorId { get; set; }

        [ForeignKey("SupervisorId")]
        public ApplicationUser? Supervisor { get; set; }

        [Required]
        public DateTime StartTime { get; set; }

        [Required]
        public DateTime EndTime { get; set; }

        public bool IsBooked { get; set; } = false;

        public ICollection<MeetingBooking>? MeetingBookings { get; set; }
    }
}