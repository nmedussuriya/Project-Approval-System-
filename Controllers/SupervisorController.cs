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
    public class SupervisorController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<ApplicationUser> _userManager;

        public SupervisorController(ApplicationDbContext context, UserManager<ApplicationUser> userManager)
        {
            _context = context;
            _userManager = userManager;
        }

        public async Task<IActionResult> Dashboard()
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

            var pendingProjects = await _context.Projects
                .Include(p => p.ResearchArea)
                .Where(p => p.Status == "Pending" && areaIds.Contains(p.ResearchAreaId))
                .ToListAsync();

            var myMatches = await _context.Matches
                .Where(m => m.SupervisorId == user.Id)
                .ToListAsync();

            ViewBag.PendingCount = pendingProjects.Count;
            ViewBag.MyInterests = myMatches.Count;
            ViewBag.ConfirmedMatches = myMatches.Count(m => m.IsIdentityRevealed);

            return View(pendingProjects);
        }

        public async Task<IActionResult> BrowseProjects()
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

            var projects = await _context.Projects
                .Include(p => p.ResearchArea)
                .Where(p => p.Status == "Pending" && areaIds.Contains(p.ResearchAreaId))
                .ToListAsync();

            return View(projects);
        }

    }
}
