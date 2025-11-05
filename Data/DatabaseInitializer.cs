using CinemaTicketSystemCore.Models;
using Microsoft.AspNetCore.Identity;

namespace CinemaTicketSystemCore.Data
{
    public static class DatabaseInitializer
    {
        // Seeds database with essential data (cinemas, admin) and optionally test data
        public static void Seed(ApplicationDbContext context, UserManager<ApplicationUser> userManager, RoleManager<IdentityRole> roleManager, bool seedTestData = false)
        {
            // Seed basic cinema structure (always created)
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

            // Create default admin user (always created for first run)
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

            // Test data seeding - controlled by SeedTestData config setting
            if (seedTestData)
            {
                // Create test users (10 regular users + 2 admins)
                var testUsers = new List<(string email, string password, string name, string surname, string phone, bool isAdmin)>
                {
                    // Regular users
                    ("user001@test.com", "Test@123", "User", "001", "1111111111", false),
                    ("user002@test.com", "Test@123", "User", "002", "1111111112", false),
                    ("user003@test.com", "Test@123", "User", "003", "1111111113", false),
                    ("user004@test.com", "Test@123", "User", "004", "1111111114", false),
                    ("user005@test.com", "Test@123", "User", "005", "1111111115", false),
                    ("user006@test.com", "Test@123", "User", "006", "1111111116", false),
                    ("user007@test.com", "Test@123", "User", "007", "1111111117", false),
                    ("user008@test.com", "Test@123", "User", "008", "1111111118", false),
                    ("user009@test.com", "Test@123", "User", "009", "1111111119", false),
                    ("user010@test.com", "Test@123", "User", "010", "1111111120", false),
                    // Admin users
                    ("admin001@test.com", "Admin@123", "Admin", "001", "2222222221", true),
                    ("admin002@test.com", "Admin@123", "Admin", "002", "2222222222", true)
                };

                // Create test users only if they don't already exist (prevents duplicates)
                foreach (var (email, password, name, surname, phone, isAdmin) in testUsers)
                {
                    if (userManager.FindByEmailAsync(email).Result == null)
                    {
                        var user = new ApplicationUser
                        {
                            UserName = email,
                            Email = email,
                            Name = name,
                            Surname = surname,
                            PhoneNumber = phone
                        };
                        
                        var result = userManager.CreateAsync(user, password).Result;
                        if (result.Succeeded && isAdmin)
                        {
                            userManager.AddToRoleAsync(user, "Admin").Wait();
                        }
                    }
                }

                // Create test screenings across all cinemas (only if no screenings exist)
                var cinemas = context.Cinemas.ToList();
                if (cinemas.Any() && !context.Screenings.Any())
                {
                    var baseDate = DateTime.Now.Date.AddDays(1); // Tomorrow
                    var screenings = new List<Screening>();
                    
                    // Screenings for Grand Cinema (Id = 1)
                    screenings.Add(new Screening { CinemaId = cinemas[0].Id, FilmTitle = "Action Hero", StartDateTime = baseDate.AddHours(14) });
                    screenings.Add(new Screening { CinemaId = cinemas[0].Id, FilmTitle = "Action Hero", StartDateTime = baseDate.AddHours(18) });
                    screenings.Add(new Screening { CinemaId = cinemas[0].Id, FilmTitle = "Space Adventure", StartDateTime = baseDate.AddHours(20) });
                    
                    // Screenings for City Theater (Id = 2)
                    screenings.Add(new Screening { CinemaId = cinemas[1].Id, FilmTitle = "Drama Queen", StartDateTime = baseDate.AddHours(15) });
                    screenings.Add(new Screening { CinemaId = cinemas[1].Id, FilmTitle = "Mystery Night", StartDateTime = baseDate.AddHours(19) });
                    
                    // Screenings for Metro Multiplex (Id = 3)
                    screenings.Add(new Screening { CinemaId = cinemas[2].Id, FilmTitle = "Comedy Hour", StartDateTime = baseDate.AddHours(13) });
                    screenings.Add(new Screening { CinemaId = cinemas[2].Id, FilmTitle = "Thriller Time", StartDateTime = baseDate.AddHours(17) });
                    screenings.Add(new Screening { CinemaId = cinemas[2].Id, FilmTitle = "Sci-Fi Chronicles", StartDateTime = baseDate.AddHours(21) });
                    
                    // Screenings for Arts Center (Id = 4)
                    screenings.Add(new Screening { CinemaId = cinemas[3].Id, FilmTitle = "Classic Film", StartDateTime = baseDate.AddHours(16) });
                    screenings.Add(new Screening { CinemaId = cinemas[3].Id, FilmTitle = "Indie Story", StartDateTime = baseDate.AddHours(20) });
                    
                    // Screenings for Mega Screen (Id = 5)
                    screenings.Add(new Screening { CinemaId = cinemas[4].Id, FilmTitle = "Blockbuster Movie", StartDateTime = baseDate.AddHours(14) });
                    screenings.Add(new Screening { CinemaId = cinemas[4].Id, FilmTitle = "Superhero Saga", StartDateTime = baseDate.AddHours(18) });
                    screenings.Add(new Screening { CinemaId = cinemas[4].Id, FilmTitle = "Epic Journey", StartDateTime = baseDate.AddDays(1).AddHours(16) });
                    
                    // Add some screenings for the day after tomorrow
                    var dayAfter = baseDate.AddDays(1);
                    screenings.Add(new Screening { CinemaId = cinemas[0].Id, FilmTitle = "Action Hero", StartDateTime = dayAfter.AddHours(14) });
                    screenings.Add(new Screening { CinemaId = cinemas[1].Id, FilmTitle = "New Release", StartDateTime = dayAfter.AddHours(18) });
                    screenings.Add(new Screening { CinemaId = cinemas[2].Id, FilmTitle = "Horror Night", StartDateTime = dayAfter.AddHours(20) });
                    
                    context.Screenings.AddRange(screenings);
                }
            }

            context.SaveChanges();
        }
    }
}

