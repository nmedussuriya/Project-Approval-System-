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
    public class AvailabilityController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<ApplicationUser> _userManager;

        public AvailabilityController(ApplicationDbContext context, UserManager<ApplicationUser> userManager)
        {
            _context = context;
            _userManager = userManager;
        }

        [Authorize(Roles = "Supervisor")]
        public async Task<IActionResult> MySlots()
        {
            var supervisorId = _userManager.GetUserId(User);

            var slots = await _context.SupervisorAvailabilities
                .Where(s => s.SupervisorId == supervisorId)
                .Include(s => s.MeetingBookings)
                    .ThenInclude(b => b.Student)
                .Include(s => s.MeetingBookings)
                    .ThenInclude(b => b.Project)
                .OrderBy(s => s.StartTime)
                .ToListAsync();

            return View(slots);
        }

        [Authorize(Roles = "Supervisor")]
        public IActionResult CreateSlot()
        {
            return View();
        }

        [HttpPost]
        [Authorize(Roles = "Supervisor")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CreateSlot(SupervisorAvailability model)
        {
            var supervisorId = _userManager.GetUserId(User);

            if (model.EndTime <= model.StartTime)
            {
                ModelState.AddModelError("", "End time must be after start time.");
            }

            if (ModelState.IsValid)
            {
                model.SupervisorId = supervisorId;
                model.IsBooked = false;

                _context.SupervisorAvailabilities.Add(model);
                await _context.SaveChangesAsync();

                return RedirectToAction(nameof(MySlots));
            }

            return View(model);
        }

        [Authorize(Roles = "Student")]
        public async Task<IActionResult> BookSlot(int projectId)
        {
            var studentId = _userManager.GetUserId(User);

            var project = await _context.Projects
                .FirstOrDefaultAsync(p => p.ProjectId == projectId && p.StudentId == studentId);

            if (project == null)
            {
                return Unauthorized();
            }

            var match = await _context.Matches
                .FirstOrDefaultAsync(m => m.ProjectId == projectId);

            if (match == null || !match.IsIdentityRevealed || project.Status != "Matched")
            {
                return Unauthorized();
            }

            var slots = await _context.SupervisorAvailabilities
                .Where(s => s.SupervisorId == match.SupervisorId && !s.IsBooked)
                .OrderBy(s => s.StartTime)
                .ToListAsync();

            ViewBag.ProjectId = projectId;
            return View(slots);
        }

        [HttpPost]
        [Authorize(Roles = "Student")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ConfirmBooking(int availabilityId, int projectId)
        {
            var studentId = _userManager.GetUserId(User);

            var project = await _context.Projects
                .FirstOrDefaultAsync(p => p.ProjectId == projectId && p.StudentId == studentId);

            if (project == null)
            {
                return Unauthorized();
            }

            var match = await _context.Matches
                .FirstOrDefaultAsync(m => m.ProjectId == projectId);

            if (match == null || !match.IsIdentityRevealed || project.Status != "Matched")
            {
                return Unauthorized();
            }

            var slot = await _context.SupervisorAvailabilities
                .FirstOrDefaultAsync(s => s.Id == availabilityId);

            if (slot == null || slot.IsBooked)
            {
                return BadRequest("This slot is no longer available.");
            }

            if (slot.SupervisorId != match.SupervisorId)
            {
                return Unauthorized();
            }

            var existingBooking = await _context.MeetingBookings
                .FirstOrDefaultAsync(b => b.ProjectId == projectId);

            if (existingBooking != null)
            {
                return BadRequest("This project already has a booked slot.");
            }

            var booking = new MeetingBooking
            {
                AvailabilityId = availabilityId,
                StudentId = studentId,
                SupervisorId = match.SupervisorId,
                ProjectId = projectId,
                Status = "Approved"
            };

            slot.IsBooked = true;

            _context.MeetingBookings.Add(booking);
            _context.SupervisorAvailabilities.Update(slot);
            await _context.SaveChangesAsync();

            TempData["Success"] = "Meeting slot booked successfully!";
            return RedirectToAction("ProjectDetails", "StudentMatch", new { id = projectId });
        }
    }
}