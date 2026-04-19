
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using PAS_Full_System.Models;

namespace PAS_Full_System.Data
{
    public class ApplicationDbContext : IdentityDbContext<ApplicationUser>
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
            : base(options)
        {
        }

        public DbSet<Project> Projects { get; set; }
        public DbSet<ResearchArea> ResearchAreas { get; set; }
        public DbSet<Match> Matches { get; set; }
        public DbSet<SupervisorAvailability> SupervisorAvailabilities { get; set; }
        public DbSet<MeetingBooking> MeetingBookings { get; set; }
        public DbSet<SupervisorExpertise> SupervisorExpertises { get; set; }
        protected override void OnModelCreating(ModelBuilder builder)
        {
            base.OnModelCreating(builder);

            builder.Entity<ResearchArea>().HasData(
                new ResearchArea { ResearchAreaId = 1, Name = "Artificial Intelligence", Description = "AI, ML, Deep Learning" },
                new ResearchArea { ResearchAreaId = 2, Name = "Web Development", Description = "ASP.NET, React, Angular" },
                new ResearchArea { ResearchAreaId = 3, Name = "Cybersecurity", Description = "Security, Encryption, Network Security" },
                new ResearchArea { ResearchAreaId = 4, Name = "Cloud Computing", Description = "AWS, Azure, Cloud Architecture" },
                new ResearchArea { ResearchAreaId = 5, Name = "Mobile Development", Description = "iOS, Android, Flutter" }
            );

            builder.Entity<SupervisorAvailability>()
                .HasOne(sa => sa.Supervisor)
                .WithMany()
                .HasForeignKey(sa => sa.SupervisorId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.Entity<MeetingBooking>()
                .HasOne(mb => mb.Student)
                .WithMany()
                .HasForeignKey(mb => mb.StudentId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.Entity<MeetingBooking>()
                .HasOne(mb => mb.Supervisor)
                .WithMany()
                .HasForeignKey(mb => mb.SupervisorId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.Entity<MeetingBooking>()
                .HasOne(mb => mb.Project)
                .WithMany()
                .HasForeignKey(mb => mb.ProjectId)
                .OnDelete(DeleteBehavior.Cascade);

            builder.Entity<MeetingBooking>()
                .HasOne(mb => mb.Availability)
                .WithMany(sa => sa.MeetingBookings)
                .HasForeignKey(mb => mb.AvailabilityId)
                .OnDelete(DeleteBehavior.Cascade);
            builder.Entity<SupervisorExpertise>()
    .HasOne(se => se.Supervisor)
    .WithMany()
    .HasForeignKey(se => se.SupervisorId)
    .OnDelete(DeleteBehavior.Cascade);

            builder.Entity<SupervisorExpertise>()
                .HasOne(se => se.ResearchArea)
                .WithMany()
                .HasForeignKey(se => se.ResearchAreaId)
                .OnDelete(DeleteBehavior.Cascade);
        }
    }
}