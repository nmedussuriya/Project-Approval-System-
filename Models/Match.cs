namespace PAS_Full_System.Models
{
    public class Match
    {
        public int MatchId { get; set; }
        public int ProjectId { get; set; }
        public string SupervisorId { get; set; } = string.Empty;
        public DateTime MatchedAt { get; set; } = DateTime.Now;
        public bool IsIdentityRevealed { get; set; } = false;

        public virtual Project? Project { get; set; }
    }
}