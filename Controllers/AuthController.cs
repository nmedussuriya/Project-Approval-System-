using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using PAS_Full_System.Models;
using PAS_Full_System.Data;
using Microsoft.EntityFrameworkCore;
using System.Threading.Tasks;
using System.Collections.Generic;
using System.Linq;

namespace PAS_Full_System.Controllers
{
    public class AuthController : Controller
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly SignInManager<ApplicationUser> _signInManager;
        private readonly RoleManager<IdentityRole> _roleManager;
        private readonly ApplicationDbContext _context;

        // Constructor to inject dependencies
        public AuthController(
            UserManager<ApplicationUser> userManager,
            SignInManager<ApplicationUser> signInManager,
            RoleManager<IdentityRole> roleManager,
            ApplicationDbContext context)
        {
            _userManager = userManager;
            _signInManager = signInManager;
            _roleManager = roleManager;
            _context = context;
        }

        [HttpPost]
        public async Task<IActionResult> Login(string Email, string Password, bool RememberMe, string ReturnUrl)
        {
            var user = await _userManager.FindByEmailAsync(Email);

            if (user == null)
            {
                TempData["Error"] = "User not found";
                return Redirect("/Identity/Account/Login");
            }

            var result = await _signInManager.PasswordSignInAsync(user, Password, RememberMe, false);

            if (result.Succeeded)
            {
                if (await _userManager.IsInRoleAsync(user, "WebMaster"))
                    return Redirect("/WebMaster/Dashboard");

                if (!string.IsNullOrEmpty(ReturnUrl))
                    return Redirect(ReturnUrl);

                if (await _userManager.IsInRoleAsync(user, "Supervisor"))
                    return Redirect("/Supervisor/Dashboard");
                else if (await _userManager.IsInRoleAsync(user, "Admin"))
                    return Redirect("/Admin/Dashboard");
                else
                    return Redirect("/Student/Dashboard");
            }

            TempData["Error"] = "Invalid password";
            return Redirect("/Identity/Account/Login");
        }

        [HttpPost]
        public async Task<IActionResult> Register(
            string FullName,
            string Email,
            string Department,
            string Password,
            string ConfirmPassword,
            string Role,
            string StudentId,
            List<int> SelectedResearchAreas)
        {
            if (Password != ConfirmPassword)
            {
                TempData["Error"] = "Passwords do not match";
                return Redirect("/Identity/Account/Register");
            }

            var existingUser = await _userManager.FindByEmailAsync(Email);
            if (existingUser != null)
            {
                TempData["Error"] = "Email already exists";
                return Redirect("/Identity/Account/Register");
            }

            if (Role == "Supervisor" && (SelectedResearchAreas == null || !SelectedResearchAreas.Any()))
            {
                TempData["Error"] = "Please select at least one research area for Supervisor.";
                return Redirect("/Identity/Account/Register");
            }

            var user = new ApplicationUser
            {
                UserName = Email,
                Email = Email,
                FullName = FullName,
                Department = Role == "Supervisor" ? "N/A" : Department
            };

            if (Role == "Student")
            {
                user.StudentId = StudentId;
            }

            var result = await _userManager.CreateAsync(user, Password);

            if (!result.Succeeded)
            {
                TempData["Error"] = string.Join(" ", result.Errors.Select(e => e.Description));
                return Redirect("/Identity/Account/Register");
            }

            string userRole = "Student";

            if (Role == "Supervisor")
            {
                userRole = "Supervisor";
            }
            else if (Role == "Admin")
            {
                userRole = "Admin";
            }
            else if (Role == "WebMaster")
            {
                userRole = "WebMaster";
            }

            if (!await _roleManager.RoleExistsAsync(userRole))
            {
                await _roleManager.CreateAsync(new IdentityRole(userRole));
            }

            await _userManager.AddToRoleAsync(user, userRole);

            if (userRole == "Supervisor" && SelectedResearchAreas != null)
            {
                foreach (var areaId in SelectedResearchAreas)
                {
                    var supervisorExpertise = new SupervisorExpertise
                    {
                        SupervisorId = user.Id,
                        ResearchAreaId = areaId
                    };

                    _context.SupervisorExpertises.Add(supervisorExpertise);
                }

                await _context.SaveChangesAsync();
            }

            await _signInManager.SignInAsync(user, isPersistent: false);

            if (userRole == "Supervisor")
                return Redirect("/Supervisor/Dashboard");
            else if (userRole == "Admin")
                return Redirect("/Admin/Dashboard");
            else if (userRole == "WebMaster")
                return Redirect("/WebMaster/Dashboard");
            else
                return Redirect("/Student/Dashboard");
        }

        public async Task<IActionResult> Logout()
        {
            await _signInManager.SignOutAsync();
            return Redirect("/");
        }
    }
}