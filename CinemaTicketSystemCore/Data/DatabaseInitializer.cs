using CinemaTicketSystemCore.Models;
using Microsoft.AspNetCore.Identity;

namespace CinemaTicketSystemCore.Data
{
    public static class DatabaseInitializer
    {
        public static void Seed(ApplicationDbContext context, UserManager<ApplicationUser> userManager, RoleManager<IdentityRole> roleManager)
        {
            // Seed Cinemas
            if (!context.Cinemas.Any())
            {
                context.Cinemas.AddRange(new[]
                {
                    new Cinema { Name = "Grand Cinema", Rows = 10, SeatsPerRow = 15 },
                    new Cinema { Name = "City Theater", Rows = 8, SeatsPerRow = 12 },
                    new Cinema { Name = "Metro Multiplex", Rows = 12, SeatsPerRow = 20 },
                    new Cinema { Name = "Arts Center", Rows = 6, SeatsPerRow = 10 },
                    new Cinema { Name = "Mega Screen", Rows = 15, SeatsPerRow = 25 }
                });
                context.SaveChanges();
            }

            // Create admin role if it doesn't exist
            if (!roleManager.RoleExistsAsync("Admin").Result)
            {
                roleManager.CreateAsync(new IdentityRole("Admin")).Wait();
            }

            // Create default admin user if no users exist
            if (!context.Users.Any())
            {
                var adminUser = new ApplicationUser
                {
                    UserName = "admin@cinema.com",
                    Email = "admin@cinema.com",
                    Name = "Admin",
                    Surname = "User",
                    PhoneNumber = "1234567890"
                };
                var result = userManager.CreateAsync(adminUser, "Admin@123").Result;
                if (result.Succeeded)
                {
                    userManager.AddToRoleAsync(adminUser, "Admin").Wait();
                }
            }

            context.SaveChanges();
        }
    }
}

