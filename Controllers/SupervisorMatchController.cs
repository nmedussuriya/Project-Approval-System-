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
    [Authorize(Roles = "Supervisor")]
    public class SupervisorMatchController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<ApplicationUser> _userManager;

        public SupervisorMatchController(ApplicationDbContext context, UserManager<ApplicationUser> userManager)
        {
            _context = context;
            _userManager = userManager;
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ExpressInterest(int projectId)
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null)
            {
                return Redirect("/Identity/Account/Login");
            }

            var project = await _context.Projects
                .Include(p => p.ResearchArea)
                .FirstOrDefaultAsync(p => p.ProjectId == projectId);

            if (project == null)
            {
                return NotFound();
            }

            var areaIds = await _context.SupervisorExpertises
                .Where(se => se.SupervisorId == user.Id)
                .Select(se => se.ResearchAreaId)
                .ToListAsync();

            if (!areaIds.Contains(project.ResearchAreaId))
            {
                return Forbid();
            }

            var existingMatch = await _context.Matches
                .FirstOrDefaultAsync(m => m.ProjectId == projectId);

            if (existingMatch != null)
            {
                TempData["Error"] = "This project already has a match request.";
                return RedirectToAction("BrowseProjects", "Supervisor");
            }

            var match = new Match
            {
                ProjectId = projectId,
                SupervisorId = user.Id,
                MatchedAt = System.DateTime.Now,
                IsIdentityRevealed = false
            };

            _context.Matches.Add(match);

            if (project.Status == "Pending")
            {
                project.Status = "Under Review";
            }

            await _context.SaveChangesAsync();

            TempData["Success"] = "Interest expressed successfully!";
            return RedirectToAction("BrowseProjects", "Supervisor");
        }

        public async Task<IActionResult> MyMatches()
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null)
            {
                return Redirect("/Identity/Account/Login");
            }

            var matches = await _context.Matches
                .Include(m => m.Project)
                .ThenInclude(p => p.ResearchArea)
                .Where(m => m.SupervisorId == user.Id)
                .OrderByDescending(m => m.MatchedAt)
                .ToListAsync();

            return View("~/Views/Supervisor/MyMatches.cshtml", matches);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ConfirmMatch(int matchId)
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null)
            {
                return Redirect("/Identity/Account/Login");
            }

            var match = await _context.Matches
                .Include(m => m.Project)
                .FirstOrDefaultAsync(m => m.MatchId == matchId && m.SupervisorId == user.Id);

            if (match == null)
            {
                return NotFound();
            }

            match.IsIdentityRevealed = true;

            if (match.Project != null)
            {
                match.Project.Status = "Matched";
            }

            await _context.SaveChangesAsync();
            TempData["Success"] = "Match confirmed! Student details revealed.";

            return RedirectToAction(nameof(MyMatches));
        }

        public async Task<IActionResult> ProjectDetails(int id)
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null)
            {
                return Redirect("/Identity/Account/Login");
            }

            var areaIds = await _context.SupervisorExpertises
                .Where(se => se.SupervisorId == user.Id)
                .Select(se => se.ResearchAreaId)
                .ToListAsync();

            var project = await _context.Projects
                .Include(p => p.ResearchArea)
                .FirstOrDefaultAsync(p => p.ProjectId == id && areaIds.Contains(p.ResearchAreaId));

            if (project == null)
            {
                return NotFound();
            }

            var match = await _context.Matches
                .FirstOrDefaultAsync(m => m.ProjectId == project.ProjectId && m.SupervisorId == user.Id);

            if (match != null && match.IsIdentityRevealed)
            {
                var student = await _userManager.FindByIdAsync(project.StudentId);
                ViewBag.Student = student;
            }
            else
            {
                ViewBag.Student = null;
            }

            return View("~/Views/Supervisor/ProjectDetails.cshtml", project);
        }
    }
}
