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
    [Authorize]
    public class ProfileController : Controller
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly ApplicationDbContext _context;

        public ProfileController(UserManager<ApplicationUser> userManager, ApplicationDbContext context)
        {
            _userManager = userManager;
            _context = context;
        }

        public async Task<IActionResult> Index()
        {
            var user = await _userManager.GetUserAsync(User);

            if (user == null)
            {
                return Redirect("/Identity/Account/Login");
            }

            var roles = await _userManager.GetRolesAsync(user);
            var role = roles.FirstOrDefault() ?? "No Role";

            ViewBag.Role = role;

            if (role == "Supervisor")
            {
                var researchAreas = await _context.SupervisorExpertises
                    .Where(se => se.SupervisorId == user.Id)
                    .Include(se => se.ResearchArea)
                    .Select(se => se.ResearchArea != null ? se.ResearchArea.Name : "")
                    .Where(name => !string.IsNullOrEmpty(name))
                    .ToListAsync();

                ViewBag.ResearchAreas = researchAreas;
            }

            return View(user);
        }
    }
}