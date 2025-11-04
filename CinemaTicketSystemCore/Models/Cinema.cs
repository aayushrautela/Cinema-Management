using System.ComponentModel.DataAnnotations;

namespace CinemaTicketSystemCore.Models
{
    public class Cinema
    {
        public int Id { get; set; }

        [Required]
        [StringLength(200)]
        public string Name { get; set; } = string.Empty;

        [Required]
        [Range(1, 50)]
        public int Rows { get; set; }

        [Required]
        [Range(1, 50)]
        public int SeatsPerRow { get; set; }

        public virtual ICollection<Screening> Screenings { get; set; } = new List<Screening>();
    }
}

