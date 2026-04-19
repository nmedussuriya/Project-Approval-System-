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
    [Authorize(Roles = "WebMaster")]
    public class WebMasterController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly RoleManager<IdentityRole> _roleManager;

        public WebMasterController(
            ApplicationDbContext context,
            UserManager<ApplicationUser> userManager,
            RoleManager<IdentityRole> roleManager)
        {
            _context = context;
            _userManager = userManager;
            _roleManager = roleManager;
        }

        // ==================== DASHBOARD ====================

        public async Task<IActionResult> Dashboard()
        {
            var totalUsers = await _userManager.Users.CountAsync();
            var totalRoles = await _roleManager.Roles.CountAsync();
            var totalProjects = await _context.Projects.CountAsync();
            var totalMatches = await _context.Matches.CountAsync();

            ViewBag.TotalUsers = totalUsers;
            ViewBag.TotalRoles = totalRoles;
            ViewBag.TotalProjects = totalProjects;
            ViewBag.TotalMatches = totalMatches;

            // Database connection status
            try
            {
                await _context.Database.CanConnectAsync();
                ViewBag.DbStatus = "Connected";
            }
            catch
            {
                ViewBag.DbStatus = "Error";
            }

            // Pending migrations count
            var pendingMigrations = (await _context.Database.GetPendingMigrationsAsync()).ToList();
            ViewBag.PendingMigrations = pendingMigrations.Count;

            // Applied migrations count
            var appliedMigrations = (await _context.Database.GetAppliedMigrationsAsync()).ToList();
            ViewBag.AppliedMigrations = appliedMigrations.Count;

            return View();
        }

        // ==================== ENVIRONMENT CONFIG ====================

        public IActionResult EnvironmentConfig()
        {
            // Safely expose non-sensitive environment info
            ViewBag.DotnetVersion = System.Runtime.InteropServices.RuntimeInformation.FrameworkDescription;
            ViewBag.OSVersion = System.Runtime.InteropServices.RuntimeInformation.OSDescription;
            ViewBag.Environment = Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT") ?? "Production";
            ViewBag.AppName = "PAS_Full_System";
            ViewBag.ServerTime = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");

            return View();
        }

        // ==================== DATABASE VERSIONING ====================

        public async Task<IActionResult> DatabaseVersioning()
        {
            var appliedMigrations = (await _context.Database.GetAppliedMigrationsAsync()).ToList();
            var pendingMigrations = (await _context.Database.GetPendingMigrationsAsync()).ToList();

            ViewBag.AppliedMigrations = appliedMigrations;
            ViewBag.PendingMigrations = pendingMigrations;

            return View();
        }

        // ==================== RBAC MANAGEMENT ====================

        public async Task<IActionResult> RoleManagement()
        {
            var roles = await _roleManager.Roles.ToListAsync();
            var roleUserCounts = new Dictionary<string, int>();

            foreach (var role in roles)
            {
                var usersInRole = await _userManager.GetUsersInRoleAsync(role.Name!);
                roleUserCounts[role.Name!] = usersInRole.Count;
            }

            ViewBag.RoleUserCounts = roleUserCounts;
            return View(roles);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CreateRole(string RoleName)
        {
            if (string.IsNullOrWhiteSpace(RoleName))
            {
                TempData["Error"] = "Role name cannot be empty.";
                return RedirectToAction(nameof(RoleManagement));
            }

            if (await _roleManager.RoleExistsAsync(RoleName))
            {
                TempData["Error"] = $"Role '{RoleName}' already exists.";
                return RedirectToAction(nameof(RoleManagement));
            }

            var result = await _roleManager.CreateAsync(new IdentityRole(RoleName));
            if (result.Succeeded)
            {
                TempData["Success"] = $"Role '{RoleName}' created successfully!";
            }
            else
            {
                TempData["Error"] = "Failed to create role: " + string.Join(", ", result.Errors.Select(e => e.Description));
            }

            return RedirectToAction(nameof(RoleManagement));
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteRole(string RoleName)
        {
            // Protect core roles from deletion
            var protectedRoles = new[] { "Admin", "Supervisor", "Student", "WebMaster" };
            if (protectedRoles.Contains(RoleName))
            {
                TempData["Error"] = $"Cannot delete the protected role '{RoleName}'.";
                return RedirectToAction(nameof(RoleManagement));
            }

            var role = await _roleManager.FindByNameAsync(RoleName);
            if (role != null)
            {
                var result = await _roleManager.DeleteAsync(role);
                if (result.Succeeded)
                {
                    TempData["Success"] = $"Role '{RoleName}' deleted successfully!";
                }
                else
                {
                    TempData["Error"] = "Failed to delete role.";
                }
            }

            return RedirectToAction(nameof(RoleManagement));
        }

        // ==================== SECURITY OVERVIEW ====================

        public async Task<IActionResult> SecurityOverview()
        {
            var users = await _userManager.Users.ToListAsync();
            var userSecurityInfo = new List<UserSecurityInfo>();

            foreach (var user in users)
            {
                var roles = await _userManager.GetRolesAsync(user);
                userSecurityInfo.Add(new UserSecurityInfo
                {
                    UserId = user.Id,
                    Email = user.Email ?? "",
                    FullName = user.FullName,
                    Role = roles.FirstOrDefault() ?? "No Role",
                    LockoutEnabled = user.LockoutEnabled,
                    LockoutEnd = user.LockoutEnd?.DateTime,
                    AccessFailedCount = user.AccessFailedCount
                });
            }

            return View(userSecurityInfo);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> LockUser(string userId)
        {
            var user = await _userManager.FindByIdAsync(userId);
            if (user != null)
            {
                await _userManager.SetLockoutEndDateAsync(user, DateTimeOffset.UtcNow.AddYears(100));
                TempData["Success"] = $"User '{user.Email}' has been locked out.";
            }
            return RedirectToAction(nameof(SecurityOverview));
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> UnlockUser(string userId)
        {
            var user = await _userManager.FindByIdAsync(userId);
            if (user != null)
            {
                await _userManager.SetLockoutEndDateAsync(user, null);
                await _userManager.ResetAccessFailedCountAsync(user);
                TempData["Success"] = $"User '{user.Email}' has been unlocked.";
            }
            return RedirectToAction(nameof(SecurityOverview));
        }
    }

    // Helper model for security view
    public class UserSecurityInfo
    {
        public string UserId { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string FullName { get; set; } = string.Empty;
        public string Role { get; set; } = string.Empty;
        public bool LockoutEnabled { get; set; }
        public DateTime? LockoutEnd { get; set; }
        public int AccessFailedCount { get; set; }
    }
}