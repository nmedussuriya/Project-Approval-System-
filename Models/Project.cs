using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace PAS_Full_System.Models
{
    public class Project
    {
        [Key]
        public int ProjectId { get; set; }

        public string Title { get; set; } = string.Empty;
        public string Abstract { get; set; } = string.Empty;
        public string TechStack { get; set; } = string.Empty;
        public string StudentId { get; set; } = string.Empty;
        public int ResearchAreaId { get; set; }
        public string Status { get; set; } = "Pending";
        public DateTime CreatedAt { get; set; } = DateTime.Now;

        // New fields for group members
        public string? GroupMemberNames { get; set; }  // Comma separated names
        public string? GroupMemberIds { get; set; }    // Comma separated student IDs

        // File upload
        public string? ProposalFilePath { get; set; }  // Path to uploaded file
        public string? ProposalFileName { get; set; }  // Original file name

        public virtual ResearchArea? ResearchArea { get; set; }
    }
}