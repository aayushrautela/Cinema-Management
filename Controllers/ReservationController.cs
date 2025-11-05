using System;
using System.Linq;
using System.Threading.Tasks;
using CinemaTicketSystemCore.Data;
using CinemaTicketSystemCore.Models;
using CinemaTicketSystemCore.Models.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace CinemaTicketSystemCore.Controllers
{
    [Authorize]
    public class ReservationController : Controller
    {
        private readonly ApplicationDbContext _db;
        private readonly UserManager<ApplicationUser> _userManager;

        public ReservationController(ApplicationDbContext db, UserManager<ApplicationUser> userManager)
        {
            _db = db;
            _userManager = userManager;
        }

        [HttpGet]
        public async Task<IActionResult> RoomView(int id)
        {
            var screening = await _db.Screenings
                .Include(s => s.Cinema)
                .Include(s => s.SeatReservations)
                .FirstOrDefaultAsync(s => s.Id == id);

            if (screening == null)
            {
                return NotFound();
            }

            var userId = _userManager.GetUserId(User);
            var viewModel = new RoomViewModel
            {
                ScreeningId = screening.Id,
                FilmTitle = screening.FilmTitle,
                CinemaName = screening.Cinema.Name,
                StartDateTime = screening.StartDateTime,
                Rows = screening.Cinema.Rows,
                SeatsPerRow = screening.Cinema.SeatsPerRow,
                CurrentUserId = userId
            };

            // Populate seat status
            foreach (var reservation in screening.SeatReservations)
            {
                string key = $"{reservation.RowNumber}_{reservation.SeatNumber}";
                viewModel.SeatStatus[key] = true;
                viewModel.SeatOwners[key] = reservation.UserId;
            }

            return View(viewModel);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ReserveSeat(int screeningId, int rowNumber, int seatNumber)
        {
            var screening = await _db.Screenings
                .Include(s => s.Cinema)
                .FirstOrDefaultAsync(s => s.Id == screeningId);

            if (screening == null)
            {
                return NotFound();
            }

            // Validate seat coordinates
            if (rowNumber < 1 || rowNumber > screening.Cinema.Rows ||
                seatNumber < 1 || seatNumber > screening.Cinema.SeatsPerRow)
            {
                TempData["ErrorMessage"] = "Invalid seat coordinates.";
                return RedirectToAction("RoomView", new { id = screeningId });
            }

            // Check if seat is already reserved
            var existingReservation = await _db.SeatReservations
                .FirstOrDefaultAsync(sr => sr.ScreeningId == screeningId &&
                                     sr.RowNumber == rowNumber &&
                                     sr.SeatNumber == seatNumber);

            if (existingReservation != null)
            {
                TempData["ErrorMessage"] = "This seat is already reserved.";
                return RedirectToAction("RoomView", new { id = screeningId });
            }

            // Create reservation with conflict handling
            try
            {
                var userId = _userManager.GetUserId(User);
                if (userId == null)
                {
                    return Unauthorized();
                }

                var reservation = new SeatReservation
                {
                    ScreeningId = screeningId,
                    UserId = userId,
                    RowNumber = rowNumber,
                    SeatNumber = seatNumber,
                    ReservedAt = DateTime.Now
                };

                _db.SeatReservations.Add(reservation);
                await _db.SaveChangesAsync();

                TempData["SuccessMessage"] = $"Seat {rowNumber}-{seatNumber} reserved successfully.";
            }
            catch (DbUpdateException ex)
            {
                // Handle unique constraint violation (concurrent reservation)
                if (ex.InnerException != null && 
                    (ex.InnerException.Message.Contains("Duplicate entry") || 
                     ex.InnerException.Message.Contains("UNIQUE constraint")))
                {
                    TempData["ErrorMessage"] = "This seat was just reserved by another user. Please select a different seat.";
                }
                else
                {
                    TempData["ErrorMessage"] = "An error occurred while reserving the seat. Please try again.";
                }
            }

            return RedirectToAction("RoomView", new { id = screeningId });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ReleaseSeat(int screeningId, int rowNumber, int seatNumber)
        {
            var userId = _userManager.GetUserId(User);
            if (userId == null)
            {
                return Unauthorized();
            }

            var reservation = await _db.SeatReservations
                .FirstOrDefaultAsync(sr => sr.ScreeningId == screeningId &&
                                     sr.RowNumber == rowNumber &&
                                     sr.SeatNumber == seatNumber &&
                                     sr.UserId == userId);

            if (reservation == null)
            {
                TempData["ErrorMessage"] = "Reservation not found or you don't have permission to cancel it.";
                return RedirectToAction("RoomView", new { id = screeningId });
            }

            _db.SeatReservations.Remove(reservation);
            await _db.SaveChangesAsync();

            TempData["SuccessMessage"] = $"Seat {rowNumber}-{seatNumber} reservation cancelled.";
            return RedirectToAction("RoomView", new { id = screeningId });
        }
    }
}

