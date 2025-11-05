using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using CinemaTicketSystemCore.Data;
using CinemaTicketSystemCore.Models;
using Microsoft.EntityFrameworkCore;

namespace CinemaTicketSystemCore.Controllers
{
    public class HomeController : Controller
    {
        private readonly ApplicationDbContext _db;
        private readonly UserManager<ApplicationUser> _userManager;

        public HomeController(ApplicationDbContext db, UserManager<ApplicationUser> userManager)
        {
            _db = db;
            _userManager = userManager;
        }

        public IActionResult Index()
        {
            return View();
        }

        [Authorize]
        public async Task<IActionResult> MyReservations()
        {
            var userId = _userManager.GetUserId(User);
            if (userId == null)
            {
                return Unauthorized();
            }

            // Load user reservations with screening and cinema details
            var reservations = await _db.SeatReservations
                .Where(r => r.UserId == userId)
                .Include(r => r.Screening)
                    .ThenInclude(s => s.Cinema)
                .OrderByDescending(r => r.Screening.StartDateTime)
                .ToListAsync();

            return View(reservations);
        }
    }
}

