using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using PAS_Full_System.Data;
using PAS_Full_System.Models;
using System.Linq;
using System.Threading.Tasks;

namespace PAS_Full_System.Controllers
{
    [Authorize(Roles = "Student")]
    public class StudentMatchController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<ApplicationUser> _userManager;

        public StudentMatchController(ApplicationDbContext context, UserManager<ApplicationUser> userManager)
        {
            _context = context;
            _userManager = userManager;
        }

        public async Task<IActionResult> ProjectDetails(int id)
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null)
            {
                return Redirect("/Identity/Account/Login");
            }

            var project = await _context.Projects
                .Include(p => p.ResearchArea)
                .FirstOrDefaultAsync(p => p.ProjectId == id && p.StudentId == user.Id);

            if (project == null)
            {
                return NotFound();
            }

            var match = await _context.Matches
                .FirstOrDefaultAsync(m => m.ProjectId == id);

            ViewBag.IsRevealed = false;
            ViewBag.Supervisor = null;
            ViewBag.AvailableSlots = new List<SupervisorAvailability>();

            if (match != null && match.IsIdentityRevealed && project.Status == "Matched")
            {
                var supervisor = await _userManager.FindByIdAsync(match.SupervisorId);

                var availableSlots = await _context.SupervisorAvailabilities
                    .Where(s => s.SupervisorId == match.SupervisorId && !s.IsBooked)
                    .OrderBy(s => s.StartTime)
                    .ToListAsync();

                ViewBag.Supervisor = supervisor;
                ViewBag.IsRevealed = true;
                ViewBag.AvailableSlots = availableSlots;
            }

            return View("~/Views/Student/ProjectDetails.cshtml", project);
        }
    }
}
