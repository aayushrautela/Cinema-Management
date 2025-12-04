using CinemaTicketSystemCore.Models;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace CinemaTicketSystemCore.Data
{
    public class ApplicationDbContext : IdentityDbContext<ApplicationUser>
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
            : base(options)
        {
        }

        public DbSet<Cinema> Cinemas { get; set; }
        public DbSet<Screening> Screenings { get; set; }
        public DbSet<SeatReservation> SeatReservations { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // Configure SeatReservation unique constraint
            modelBuilder.Entity<SeatReservation>()
                .HasIndex(s => new { s.ScreeningId, s.RowNumber, s.SeatNumber })
                .IsUnique()
                .HasDatabaseName("IX_SeatReservation_Unique");

            // Configure relationships
            modelBuilder.Entity<Screening>()
                .HasOne(s => s.Cinema)
                .WithMany(c => c.Screenings)
                .HasForeignKey(s => s.CinemaId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<SeatReservation>()
                .HasOne(sr => sr.Screening)
                .WithMany(s => s.SeatReservations)
                .HasForeignKey(sr => sr.ScreeningId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<SeatReservation>()
                .HasOne(sr => sr.User)
                .WithMany()
                .HasForeignKey(sr => sr.UserId)
                .OnDelete(DeleteBehavior.Restrict);

            // Configure concurrency token for ApplicationUser
            modelBuilder.Entity<ApplicationUser>()
                .Property(u => u.LockVersion)
                .IsConcurrencyToken();
        }
    }
}

