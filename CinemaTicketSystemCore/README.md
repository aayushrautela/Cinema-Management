# Cinema Ticket Purchasing System - ASP.NET Core

An ASP.NET Core MVC application for online cinema ticket purchasing with seat reservation functionality.

## Features

- **User Registration & Management**: Users can register and edit their profiles (name, surname, phone number)
- **Admin Screening Management**: Administrators can create and delete film screenings
- **Seat Reservation**: Users can view room occupancy and reserve/release seats
- **Conflict Handling**: Database-level unique constraints prevent duplicate seat reservations
- **Concurrency Control**: Optimistic locking prevents concurrent user profile edits from overwriting each other
- **Bootstrap UI**: Responsive design using Bootstrap 5
- **Cross-Platform**: Runs on Linux, Windows, and macOS

## Technology Stack

- ASP.NET Core 8.0
- Entity Framework Core 8.0
- MySQL Database (using Pomelo.EntityFrameworkCore.MySql)
- ASP.NET Core Identity for authentication
- Bootstrap 5 for UI styling

## Prerequisites

- .NET 8.0 SDK or later
- MySQL Server 8.0 or later

## Setup Instructions

### 1. Install .NET SDK

```bash
# On Linux (Fedora/RHEL)
sudo dnf install dotnet-sdk-8.0

# Verify installation
dotnet --version
```

### 2. Install MySQL

```bash
# On Linux (Fedora/RHEL)
sudo dnf install mysql-server
sudo systemctl start mysqld
sudo systemctl enable mysqld
sudo mysql_secure_installation
```

Alternatively, use the provided setup script:

```bash
cd CinemaTicketSystemCore
./scripts/setup-mysql-simple.sh
```

### 3. Configure Database Connection

Edit `appsettings.json` and update the connection string:

```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Server=localhost;Database=CinemaDB;Uid=cinemauser;Pwd=coolpass;CharSet=utf8mb4;"
  },
  "SeedTestData": false
}
```

The `SeedTestData` setting controls whether test data (users and screenings) is seeded on startup.

### 4. Build and Run

```bash
cd CinemaTicketSystemCore
dotnet restore
dotnet build
dotnet run
```

The application will start on `http://localhost:5000` or `https://localhost:5001`

### 5. Database Auto-Creation

On first run, Entity Framework will:
- Create the `CinemaDB` database if it doesn't exist
- Create all tables
- Seed initial data:
  - 5 cinemas with different room sizes
  - Admin role
  - Admin user: `admin@cinema.com` / `Admin@123`
  
If `SeedTestData` is set to `true`, additional test users and screenings will be created.

## Usage

1. Register a new user or login as admin
2. Admin functions:
   - Login as `admin@cinema.com` / `Admin@123`
   - Create new screenings
   - Delete screenings (cascades reservations)
   - Edit user profiles
   - Delete users
3. User functions:
   - Select a screening
   - View room occupancy
   - Reserve seats
   - Cancel own reservations
   - Edit own profile

## Project Structure

```
CinemaTicketSystemCore/
├── Controllers/          # MVC Controllers
├── Models/              # Data models and ViewModels
├── Views/               # Razor views
├── Data/                # DbContext and database initialization
├── Filters/             # Authorization filters
├── wwwroot/             # Static files (CSS, JS)
├── scripts/             # Setup scripts
├── Program.cs           # Application entry point
└── appsettings.json     # Configuration
```

## Configuration

### Test Data Seeding

Set `SeedTestData` in `appsettings.json` to control test data seeding:
- `true`: Creates 10 test users, 2 test admin users, and 15 test screenings
- `false`: Creates only essential data (cinemas, admin role, default admin user)

To reset the database:

```bash
mysql -u cinemauser -pcoolpass -e "DROP DATABASE IF EXISTS CinemaDB; CREATE DATABASE CinemaDB CHARACTER SET utf8mb4 COLLATE utf8mb4_unicode_ci;"
```

Then restart the application to reinitialize the database.

## Architecture Notes

- Uses ASP.NET Core Identity for user authentication and authorization
- Entity Framework Core with MySQL for data persistence
- Optimistic concurrency control using LockVersion for user profile edits
- Database-level unique constraints prevent duplicate seat reservations
- Cascade delete configured for screenings and reservations

## Troubleshooting

- **MySQL connection error**: Ensure MySQL is running and connection string is correct
- **Database not created**: Check MySQL permissions and connection string
- **Build errors**: Run `dotnet restore` to restore NuGet packages
- **Concurrency errors**: If editing a user profile fails with a concurrency error, refresh the page and try again

## License

This project is for educational purposes.

