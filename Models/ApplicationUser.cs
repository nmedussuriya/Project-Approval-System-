using Microsoft.AspNetCore.Identity;

namespace PAS_Full_System.Models
{
    public class ApplicationUser : IdentityUser
    {
        public string FullName { get; set; } = string.Empty;
        public string Department { get; set; } = string.Empty;
        public DateTime CreatedAt { get; set; } = DateTime.Now;
        public string? StudentId { get; set; }
       
    }
}