using System;
using System.Linq;
using System.Threading.Tasks;
using CinemaTicketSystemCore.Data;
using CinemaTicketSystemCore.Models;
using CinemaTicketSystemCore.Models.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Identity;

namespace CinemaTicketSystemCore.Controllers
{
    [Authorize(Roles = "Admin")]
    public class AdminController : Controller
    {
        private readonly ApplicationDbContext _db;
        private readonly UserManager<ApplicationUser> _userManager;

        public AdminController(ApplicationDbContext db, UserManager<ApplicationUser> userManager)
        {
            _db = db;
            _userManager = userManager;
        }

        [HttpGet]
        public async Task<IActionResult> CreateScreening()
        {
            try
            {
                var cinemas = await _db.Cinemas.ToListAsync();
                ViewBag.Cinemas = new SelectList(cinemas, "Id", "Name");
                return View(new CreateScreeningViewModel());
            }
            catch (Exception ex)
            {
                // Log error and return error view
                return StatusCode(500, $"Error loading cinemas: {ex.Message}");
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CreateScreening(CreateScreeningViewModel model)
        {
            if (ModelState.IsValid)
            {
                var cinema = await _db.Cinemas.FindAsync(model.CinemaId);
                if (cinema == null)
                {
                    ModelState.AddModelError("", "Selected cinema not found.");
                    ViewBag.Cinemas = new SelectList(await _db.Cinemas.ToListAsync(), "Id", "Name");
                    return View(model);
                }

                // Combine date and time into DateTime
                var startDateTime = model.StartDate.Date.Add(model.StartTime);

                var screening = new Screening
                {
                    CinemaId = model.CinemaId,
                    FilmTitle = model.FilmTitle,
                    StartDateTime = startDateTime
                };

                _db.Screenings.Add(screening);
                await _db.SaveChangesAsync();

                return RedirectToAction("ScreeningsList");
            }

            ViewBag.Cinemas = new SelectList(await _db.Cinemas.ToListAsync(), "Id", "Name");
            return View(model);
        }

        [HttpGet]
        public async Task<IActionResult> ScreeningsList()
        {
            var screenings = await _db.Screenings
                .Include(s => s.Cinema)
                .OrderBy(s => s.StartDateTime)
                .Select(s => new ScreeningViewModel
                {
                    Id = s.Id,
                    FilmTitle = s.FilmTitle,
                    CinemaName = s.Cinema.Name,
                    StartDateTime = s.StartDateTime,
                    CinemaId = s.CinemaId
                })
                .ToListAsync();

            return View(screenings);
        }

        [HttpGet]
        public async Task<IActionResult> DeleteScreening(int id)
        {
            var screening = await _db.Screenings
                .Include(s => s.Cinema)
                .Include(s => s.SeatReservations)
                .FirstOrDefaultAsync(s => s.Id == id);

            if (screening == null)
            {
                return NotFound();
            }

            return View(screening);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [ActionName("DeleteScreening")]
        public async Task<IActionResult> DeleteScreeningConfirmed(int id)
        {
            var screening = await _db.Screenings
                .Include(s => s.SeatReservations)
                .FirstOrDefaultAsync(s => s.Id == id);

            if (screening == null)
            {
                return NotFound();
            }

            // Cascade delete reservations is handled by EF configuration
            _db.Screenings.Remove(screening);
            await _db.SaveChangesAsync();

            TempData["SuccessMessage"] = $"Screening '{screening.FilmTitle}' has been deleted successfully. All reservations for this screening have been removed.";
            return RedirectToAction("ScreeningsList");
        }

        [HttpGet]
        public async Task<IActionResult> UsersList()
        {
            var users = await _db.Users
                .OrderBy(u => u.Email)
                .Select(u => new UserSummaryViewModel
                {
                    Id = u.Id,
                    Email = u.Email ?? string.Empty,
                    Name = u.Name,
                    Surname = u.Surname,
                    PhoneNumber = u.PhoneNumber
                })
                .ToListAsync();

            // Determine which users are admins
            var adminRoleId = await _db.Roles.Where(r => r.Name == "Admin").Select(r => r.Id).FirstOrDefaultAsync();
            if (!string.IsNullOrEmpty(adminRoleId))
            {
                var adminUserIds = await _db.UserRoles
                    .Where(ur => ur.RoleId == adminRoleId)
                    .Select(ur => ur.UserId)
                    .ToListAsync();

                var adminSet = new HashSet<string>(adminUserIds);
                foreach (var u in users)
                {
                    u.IsAdmin = adminSet.Contains(u.Id);
                }
            }

            return View(users);
        }

        [HttpGet]
        public async Task<IActionResult> DeleteUser(string id)
        {
            if (string.IsNullOrWhiteSpace(id))
            {
                return NotFound();
            }

            // Load user to show deletion confirmation
            // Note: User may be deleted by another admin between GET and POST (handled in POST)
            var user = await _db.Users.FirstOrDefaultAsync(u => u.Id == id);
            if (user == null)
            {
                return NotFound();
            }

            // Count reservations for display (may change if user makes reservations concurrently)
            var reservationCount = await _db.SeatReservations.CountAsync(r => r.UserId == id);

            var vm = new DeleteUserViewModel
            {
                Id = user.Id,
                Email = user.Email ?? string.Empty,
                Name = user.Name,
                Surname = user.Surname,
                PhoneNumber = user.PhoneNumber,
                ReservationCount = reservationCount
            };

            return View(vm);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [ActionName("DeleteUser")]
        public async Task<IActionResult> DeleteUserConfirmed(string id)
        {
            if (string.IsNullOrWhiteSpace(id))
            {
                return NotFound();
            }

            // Reload user from database - critical for parallel deletion safety
            // Race condition: Another admin may have deleted this user between GET and POST
            // Also, user may have been deleted while showing confirmation page
            var user = await _db.Users.FirstOrDefaultAsync(u => u.Id == id);
            if (user == null)
            {
                // User already deleted by another process (parallel deletion scenario)
                TempData["ErrorMessage"] = "User was already deleted by another administrator.";
                return RedirectToAction("UsersList");
            }

            // Remove user's reservations first (required due to FK constraint)
            // Note: Reservations may be added/deleted concurrently, but RemoveRange handles all matching records
            var reservations = _db.SeatReservations.Where(r => r.UserId == id);
            _db.SeatReservations.RemoveRange(reservations);
            await _db.SaveChangesAsync();

            // Delete user using UserManager (ensures Identity cleanup + proper transaction handling)
            // This operation is atomic - if another admin deletes concurrently, DeleteAsync will handle it
            var result = await _userManager.DeleteAsync(user);
            if (!result.Succeeded)
            {
                // Possible race condition: user deleted between reservation removal and user deletion
                TempData["ErrorMessage"] = string.Join("; ", result.Errors.Select(e => e.Description));
                return RedirectToAction("DeleteUser", new { id });
            }

            TempData["SuccessMessage"] = $"User '{user.Email}' deleted.";
            return RedirectToAction("UsersList");
        }
    }

    public class CreateScreeningViewModel
    {
        [System.ComponentModel.DataAnnotations.Required]
        [System.ComponentModel.DataAnnotations.Display(Name = "Cinema")]
        public int CinemaId { get; set; }

        [System.ComponentModel.DataAnnotations.Required]
        [System.ComponentModel.DataAnnotations.StringLength(200)]
        [System.ComponentModel.DataAnnotations.Display(Name = "Film Title")]
        public string FilmTitle { get; set; } = string.Empty;

        [System.ComponentModel.DataAnnotations.Required]
        [System.ComponentModel.DataAnnotations.Display(Name = "Start Date")]
        [System.ComponentModel.DataAnnotations.DataType(System.ComponentModel.DataAnnotations.DataType.Date)]
        public DateTime StartDate { get; set; }

        [System.ComponentModel.DataAnnotations.Required]
        [System.ComponentModel.DataAnnotations.Display(Name = "Start Time")]
        [System.ComponentModel.DataAnnotations.DataType(System.ComponentModel.DataAnnotations.DataType.Time)]
        public TimeSpan StartTime { get; set; }
    }

    public class UserSummaryViewModel
    {
        public string Id { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string Name { get; set; } = string.Empty;
        public string Surname { get; set; } = string.Empty;
        public string? PhoneNumber { get; set; }
        public bool IsAdmin { get; set; }
    }

    public class DeleteUserViewModel
    {
        public string Id { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string Name { get; set; } = string.Empty;
        public string Surname { get; set; } = string.Empty;
        public string? PhoneNumber { get; set; }
        public int ReservationCount { get; set; }
    }
}

