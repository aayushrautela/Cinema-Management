using System.Linq;
using System.Threading.Tasks;
using CinemaTicketSystemCore.Data;
using CinemaTicketSystemCore.Models.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace CinemaTicketSystemCore.Controllers
{
    [Authorize]
    public class ScreeningController : Controller
    {
        private readonly ApplicationDbContext _db;

        public ScreeningController(ApplicationDbContext db)
        {
            _db = db;
        }

        [HttpGet]
        public async Task<IActionResult> SelectScreening()
        {
            var screenings = await _db.Screenings
                .Include(s => s.Cinema)
                .Where(s => s.StartDateTime > DateTime.Now)
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
    }
}

