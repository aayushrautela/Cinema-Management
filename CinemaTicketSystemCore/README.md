# Cinema Ticket Purchasing System - ASP.NET Core

An ASP.NET Core MVC application for online cinema ticket purchasing with seat reservation functionality.

## Features

- **User Registration & Management**: Users can register and edit their profiles (name, surname, phone number)
- **Admin Screening Management**: Administrators can create and delete film screenings
- **Seat Reservation**: Users can view room occupancy and reserve/release seats
- **Conflict Handling**: Database-level unique constraints prevent duplicate seat reservations
- **Concurrency Control**: Optimistic locking prevents concurrent user profile edits from overwriting each other
- **Bootstrap UI**: Responsive design using Bootstrap 5
- **Cross-Platform**: Runs on Linux, Windows, and macOS!

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

### 3. Configure Database Connection

Edit `appsettings.json` and update the connection string:

```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Server=localhost;Database=CinemaDB;Uid=root;Pwd=YOUR_PASSWORD;CharSet=utf8mb4;"
  }
}
```

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
- Create the `CinemaDB` database
- Create all tables
- Seed initial data:
  - 5 cinemas with different room sizes
  - Admin user: `admin@cinema.com` / `Admin@123`

## Usage

1. **Register a new user** or login as admin
2. **Admin functions**:
   - Login as `admin@cinema.com` / `Admin@123`
   - Create new screenings
   - Delete screenings (cascades reservations)
3. **User functions**:
   - Select a screening
   - View room occupancy
   - Reserve seats
   - Cancel own reservations

## Project Structure

```
CinemaTicketSystemCore/
├── Controllers/          # MVC Controllers
├── Models/              # Data models and ViewModels
├── Views/               # Razor views
├── Data/                # DbContext and database initialization
├── wwwroot/             # Static files (CSS, JS)
├── Program.cs           # Application entry point
└── appsettings.json     # Configuration
```

## Key Differences from .NET Framework Version

- ✅ Runs on Linux, Windows, macOS
- ✅ Uses ASP.NET Core Identity
- ✅ Entity Framework Core (not EF6)
- ✅ Modern dependency injection
- ✅ Tag helpers in views (instead of HTML helpers)
- ✅ appsettings.json (instead of Web.config)

## Troubleshooting

- **MySQL connection error**: Ensure MySQL is running and connection string is correct
- **Database not created**: Check MySQL permissions and connection string
- **Build errors**: Run `dotnet restore` to restore NuGet packages

## License

This project is for educational purposes.

