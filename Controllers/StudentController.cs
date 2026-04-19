using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using PAS_Full_System.Data;
using PAS_Full_System.Models;
using System.Linq;
using System.Threading.Tasks;

namespace PAS_Full_System.Controllers
{
    [Authorize(Roles = "Student")]
    public class StudentController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly IWebHostEnvironment _webHostEnvironment;

        public StudentController(
            ApplicationDbContext context,
            UserManager<ApplicationUser> userManager,
            IWebHostEnvironment webHostEnvironment)
        {
            _context = context;
            _userManager = userManager;
            _webHostEnvironment = webHostEnvironment;
        }

        public async Task<IActionResult> Dashboard()
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null)
            {
                return Redirect("/Identity/Account/Login");
            }

            var myProjects = await _context.Projects
                .Include(p => p.ResearchArea)
                .Where(p => p.StudentId == user.Id)
                .OrderByDescending(p => p.CreatedAt)
                .ToListAsync();

            ViewBag.ProjectCount = myProjects.Count;
            ViewBag.PendingCount = myProjects.Count(p => p.Status == "Pending");
            ViewBag.MatchedCount = myProjects.Count(p => p.Status == "Matched");

            return View(myProjects);
        }

        public async Task<IActionResult> CreateProject()
        {
            ViewBag.ResearchAreas = new SelectList(
                await _context.ResearchAreas.ToListAsync(),
                "ResearchAreaId",
                "Name"
            );

            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CreateProject(Project project, string GroupMemberNames, string GroupMemberIds, IFormFile ProposalFile)
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null)
            {
                return Redirect("/Identity/Account/Login");
            }

            if (!ModelState.IsValid)
            {
                ViewBag.ResearchAreas = new SelectList(
                    await _context.ResearchAreas.ToListAsync(),
                    "ResearchAreaId",
                    "Name",
                    project.ResearchAreaId
                );

                return View(project);
            }

            project.StudentId = user.Id;
            project.Status = "Pending";
            project.CreatedAt = DateTime.Now;
            project.GroupMemberNames = GroupMemberNames;
            project.GroupMemberIds = GroupMemberIds;

            if (ProposalFile != null && ProposalFile.Length > 0)
            {
                string uploadsFolder = Path.Combine(_webHostEnvironment.WebRootPath, "uploads");

                if (!Directory.Exists(uploadsFolder))
                    Directory.CreateDirectory(uploadsFolder);

                string uniqueFileName = Guid.NewGuid() + "_" + ProposalFile.FileName;
                string filePath = Path.Combine(uploadsFolder, uniqueFileName);

                using (var fileStream = new FileStream(filePath, FileMode.Create))
                {
                    await ProposalFile.CopyToAsync(fileStream);
                }

                project.ProposalFilePath = "/uploads/" + uniqueFileName;
                project.ProposalFileName = ProposalFile.FileName;
            }

            _context.Projects.Add(project);
            await _context.SaveChangesAsync();

            TempData["Success"] = "Project submitted successfully!";
            return RedirectToAction(nameof(Dashboard));
        }

        public async Task<IActionResult> EditProject(int id)
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null)
            {
                return Redirect("/Identity/Account/Login");
            }

            var project = await _context.Projects
                .FirstOrDefaultAsync(p => p.ProjectId == id && p.StudentId == user.Id && p.Status == "Pending");

            if (project == null)
            {
                return NotFound();
            }

            ViewBag.ResearchAreas = new SelectList(
                await _context.ResearchAreas.ToListAsync(),
                "ResearchAreaId",
                "Name",
                project.ResearchAreaId
            );

            return View(project);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> EditProject(int id, Project updatedProject, string GroupMemberNames, string GroupMemberIds, IFormFile ProposalFile)
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null)
            {
                return Redirect("/Identity/Account/Login");
            }

            var project = await _context.Projects
                .FirstOrDefaultAsync(p => p.ProjectId == id && p.StudentId == user.Id && p.Status == "Pending");

            if (project == null)
            {
                return NotFound();
            }

            if (!ModelState.IsValid)
            {
                ViewBag.ResearchAreas = new SelectList(
                    await _context.ResearchAreas.ToListAsync(),
                    "ResearchAreaId",
                    "Name",
                    updatedProject.ResearchAreaId
                );

                return View(updatedProject);
            }

            project.Title = updatedProject.Title;
            project.Abstract = updatedProject.Abstract;
            project.TechStack = updatedProject.TechStack;
            project.ResearchAreaId = updatedProject.ResearchAreaId;
            project.GroupMemberNames = GroupMemberNames;
            project.GroupMemberIds = GroupMemberIds;

            if (ProposalFile != null && ProposalFile.Length > 0)
            {
                string uploadsFolder = Path.Combine(_webHostEnvironment.WebRootPath, "uploads");

                if (!Directory.Exists(uploadsFolder))
                    Directory.CreateDirectory(uploadsFolder);

                string uniqueFileName = Guid.NewGuid() + "_" + ProposalFile.FileName;
                string filePath = Path.Combine(uploadsFolder, uniqueFileName);

                using (var fileStream = new FileStream(filePath, FileMode.Create))
                {
                    await ProposalFile.CopyToAsync(fileStream);
                }

                project.ProposalFilePath = "/uploads/" + uniqueFileName;
                project.ProposalFileName = ProposalFile.FileName;
            }

            await _context.SaveChangesAsync();
            TempData["Success"] = "Project updated successfully!";
            return RedirectToAction(nameof(Dashboard));
        }

        public async Task<IActionResult> MyProjects()
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null)
            {
                return Redirect("/Identity/Account/Login");
            }

            var projects = await _context.Projects
                .Include(p => p.ResearchArea)
                .Where(p => p.StudentId == user.Id)
                .OrderByDescending(p => p.CreatedAt)
                .ToListAsync();

            return View(projects);
        }

    }
}
