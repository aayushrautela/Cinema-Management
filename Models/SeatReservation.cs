using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace CinemaTicketSystemCore.Models
{
    public class SeatReservation
    {
        public int Id { get; set; }

        [Required]
        public int ScreeningId { get; set; }

        [Required]
        [StringLength(450)]
        public string UserId { get; set; } = string.Empty;

        [Required]
        [Range(1, 50)]
        public int RowNumber { get; set; }

        [Required]
        [Range(1, 50)]
        public int SeatNumber { get; set; }

        [Required]
        public DateTime ReservedAt { get; set; }

        [ForeignKey("ScreeningId")]
        public virtual Screening Screening { get; set; } = null!;

        [ForeignKey("UserId")]
        public virtual ApplicationUser User { get; set; } = null!;
    }
}

