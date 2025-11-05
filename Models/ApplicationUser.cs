using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Identity;

namespace CinemaTicketSystemCore.Models
{
    public class ApplicationUser : IdentityUser
    {
        [Required]
        [StringLength(100)]
        public string Name { get; set; } = string.Empty;

        [Required]
        [StringLength(100)]
        public string Surname { get; set; } = string.Empty;

        // Note: PhoneNumber is inherited from IdentityUser, but we can add validation attributes
        // Using 'new' to explicitly hide the base member since we're adding validation
        [StringLength(20)]
        public new string? PhoneNumber { get; set; }

        [Timestamp]
        [ConcurrencyCheck]
        public byte[]? LockVersion { get; set; }
    }
}

