using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using PAS_Full_System.Data;
using PAS_Full_System.Models;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace PAS_Full_System.Controllers
{
    [Authorize]
    public class AdminController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly RoleManager<IdentityRole> _roleManager;

        public AdminController(
            ApplicationDbContext context,
            UserManager<ApplicationUser> userManager,
            RoleManager<IdentityRole> roleManager)
        {
            _context = context;
            _userManager = userManager;
            _roleManager = roleManager;
        }

        // GET: Admin/Dashboard
        public async Task<IActionResult> Dashboard()
        {
            var totalUsers = await _userManager.Users.CountAsync();
            var totalProjects = await _context.Projects.CountAsync();
            var totalMatches = await _context.Matches.CountAsync();
            var pendingProjects = await _context.Projects.CountAsync(p => p.Status == "Pending");

            ViewBag.TotalUsers = totalUsers;
            ViewBag.TotalProjects = totalProjects;
            ViewBag.TotalMatches = totalMatches;
            ViewBag.PendingProjects = pendingProjects;

            return View();
        }

        // ==================== RESEARCH AREA MANAGEMENT ====================

        public async Task<IActionResult> ResearchAreas()
        {
            var researchAreas = await _context.ResearchAreas.ToListAsync();
            return View(researchAreas);
        }

        public IActionResult CreateResearchArea()
        {
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CreateResearchArea(ResearchArea researchArea)
        {
            if (ModelState.IsValid)
            {
                _context.ResearchAreas.Add(researchArea);
                await _context.SaveChangesAsync();
                TempData["Success"] = "Research Area created successfully!";
                return RedirectToAction(nameof(ResearchAreas));
            }

            return View(researchArea);
        }

        public async Task<IActionResult> EditResearchArea(int id)
        {
            var researchArea = await _context.ResearchAreas.FindAsync(id);
            if (researchArea == null)
            {
                return NotFound();
            }

            return View(researchArea);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> EditResearchArea(int id, ResearchArea researchArea)
        {
            if (id != researchArea.ResearchAreaId)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                _context.Update(researchArea);
                await _context.SaveChangesAsync();
                TempData["Success"] = "Research Area updated successfully!";
                return RedirectToAction(nameof(ResearchAreas));
            }

            return View(researchArea);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteResearchArea(int id)
        {
            var researchArea = await _context.ResearchAreas.FindAsync(id);
            if (researchArea != null)
            {
                _context.ResearchAreas.Remove(researchArea);
                await _context.SaveChangesAsync();
                TempData["Success"] = "Research Area deleted successfully!";
            }

            return RedirectToAction(nameof(ResearchAreas));
        }

        // ==================== USER MANAGEMENT ====================
        public async Task<IActionResult> Users()
        {
            var users = await _userManager.Users.ToListAsync();
            var userRoles = new Dictionary<string, string>();

            foreach (var user in users)
            {
                var roles = await _userManager.GetRolesAsync(user);
                userRoles[user.Id] = roles.FirstOrDefault() ?? "No Role";
            }

            ViewBag.UserRoles = userRoles;
            return View(users);
        }

        public async Task<IActionResult> EditUser(string id)
        {
            var user = await _userManager.FindByIdAsync(id);
            if (user == null)
            {
                return NotFound();
            }

            var roles = await _roleManager.Roles.ToListAsync();
            var userRoles = await _userManager.GetRolesAsync(user);
            ViewBag.Roles = roles;
            ViewBag.CurrentRole = userRoles.FirstOrDefault();

            return View(user);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> EditUser(string id, string FullName, string Department, string Role)
        {
            var user = await _userManager.FindByIdAsync(id);
            if (user == null)
            {
                return NotFound();
            }

            user.FullName = FullName;
            user.Department = Department;

            var updateResult = await _userManager.UpdateAsync(user);

            if (updateResult.Succeeded)
            {
                var currentRoles = await _userManager.GetRolesAsync(user);
                await _userManager.RemoveFromRolesAsync(user, currentRoles);
                await _userManager.AddToRoleAsync(user, Role);

                TempData["Success"] = "User updated successfully!";
            }
            else
            {
                TempData["Error"] = "Failed to update user.";
            }

            return RedirectToAction(nameof(Users));
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteUser(string id)
        {
            var user = await _userManager.FindByIdAsync(id);

            if (user == null)
            {
                return NotFound();
            }

            // Optional safety: stop admin from deleting themselves
            var currentUserId = _userManager.GetUserId(User);
            if (currentUserId == id)
            {
                TempData["Error"] = "You cannot delete your own account.";
                return RedirectToAction(nameof(Users));
            }

            // Delete related supervisor availability records first
            var supervisorSlots = await _context.SupervisorAvailabilities
                .Where(s => s.SupervisorId == id)
                .ToListAsync();

            if (supervisorSlots.Any())
            {
                // Delete bookings linked to those slots first
                var slotIds = supervisorSlots.Select(s => s.Id).ToList();

                var slotBookings = await _context.MeetingBookings
                    .Where(b => slotIds.Contains(b.AvailabilityId))
                    .ToListAsync();

                if (slotBookings.Any())
                {
                    _context.MeetingBookings.RemoveRange(slotBookings);
                }

                _context.SupervisorAvailabilities.RemoveRange(supervisorSlots);
            }

            // Delete bookings where this user is the student
            var studentBookings = await _context.MeetingBookings
                .Where(b => b.StudentId == id)
                .ToListAsync();

            if (studentBookings.Any())
            {
                _context.MeetingBookings.RemoveRange(studentBookings);
            }

            // Delete bookings where this user is the supervisor
            var supervisorBookings = await _context.MeetingBookings
                .Where(b => b.SupervisorId == id)
                .ToListAsync();
            if (supervisorBookings.Any())
            {
                _context.MeetingBookings.RemoveRange(supervisorBookings);
            }

            // Delete supervisor expertise records
            var expertise = await _context.SupervisorExpertises
                .Where(e => e.SupervisorId == id)
                .ToListAsync();

            if (expertise.Any())
            {
                _context.SupervisorExpertises.RemoveRange(expertise);
            }

            // Delete matches where this user is the supervisor
            var supervisorMatches = await _context.Matches
                .Where(m => m.SupervisorId == id)
                .ToListAsync();

            if (supervisorMatches.Any())
            {
                _context.Matches.RemoveRange(supervisorMatches);
            }

            // Delete projects owned by this user if they are a student
            var studentProjects = await _context.Projects
                .Where(p => p.StudentId == id)
                .ToListAsync();

            if (studentProjects.Any())
            {
                var studentProjectIds = studentProjects.Select(p => p.ProjectId).ToList();

                // Delete bookings linked to those projects
                var projectBookings = await _context.MeetingBookings
                    .Where(b => studentProjectIds.Contains(b.ProjectId))
                    .ToListAsync();

                if (projectBookings.Any())
                {
                    _context.MeetingBookings.RemoveRange(projectBookings);
                }

                // Delete matches linked to those projects
                var projectMatches = await _context.Matches
                    .Where(m => studentProjectIds.Contains(m.ProjectId))
                    .ToListAsync();

                if (projectMatches.Any())
                {
                    _context.Matches.RemoveRange(projectMatches);
                }

                _context.Projects.RemoveRange(studentProjects);
            }

            await _context.SaveChangesAsync();

            var result = await _userManager.DeleteAsync(user);

            if (result.Succeeded)
            {
                TempData["Success"] = "User deleted successfully!";
            }
            else
            {
                TempData["Error"] = "Failed to delete user.";
            }

            return RedirectToAction(nameof(Users));
        }

        // ==================== ALLOCATION OVERSIGHT ====================

        public async Task<IActionResult> AllMatches()
        {
            var matches = await _context.Matches
                .Include(m => m.Project)
                .ThenInclude(p => p.ResearchArea)
                .OrderByDescending(m => m.MatchedAt)
                .ToListAsync();

            var supervisors = new Dictionary<string, ApplicationUser>();
            var students = new Dictionary<string, ApplicationUser>();

            foreach (var match in matches)
            {
                if (!supervisors.ContainsKey(match.SupervisorId))
                {
                    supervisors[match.SupervisorId] = await _userManager.FindByIdAsync(match.SupervisorId);
                }

                if (match.Project != null && !students.ContainsKey(match.Project.StudentId))
                {
                    students[match.Project.StudentId] = await _userManager.FindByIdAsync(match.Project.StudentId);
                }
            }

            ViewBag.Supervisors = supervisors;
            ViewBag.Students = students;

            return View(matches);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ReassignProject(int matchId, string newSupervisorId)
        {
            var match = await _context.Matches.FindAsync(matchId);
            if (match != null)
            {
                match.SupervisorId = newSupervisorId;
                await _context.SaveChangesAsync();
                TempData["Success"] = "Project reassigned successfully!";
            }
            return RedirectToAction(nameof(AllMatches));
        }
    }
}