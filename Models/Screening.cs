using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace CinemaTicketSystemCore.Models
{
    public class Screening
    {
        public int Id { get; set; }

        [Required]
        public int CinemaId { get; set; }

        [Required]
        [StringLength(200)]
        public string FilmTitle { get; set; } = string.Empty;

        [Required]
        public DateTime StartDateTime { get; set; }

        [ForeignKey("CinemaId")]
        public virtual Cinema Cinema { get; set; } = null!;

        public virtual ICollection<SeatReservation> SeatReservations { get; set; } = new List<SeatReservation>();
    }
}

