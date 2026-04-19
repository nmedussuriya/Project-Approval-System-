using System.ComponentModel.DataAnnotations.Schema;

namespace PAS_Full_System.Models
{
    public class SupervisorExpertise
    {
        public int Id { get; set; }

        public string SupervisorId { get; set; } = string.Empty;

        [ForeignKey("SupervisorId")]
        public ApplicationUser? Supervisor { get; set; }

        public int ResearchAreaId { get; set; }

        [ForeignKey("ResearchAreaId")]
        public ResearchArea? ResearchArea { get; set; }
    }
}