# Quick Start Guide

## Automatic Setup (Recommended)

Run the setup script to automatically configure MySQL with password 'coolpass':

```bash
cd /home/aayush/egui1/CinemaTicketSystemCore
./scripts/setup-mysql.sh
```

This script will:
- Set MySQL root password to 'coolpass'
- Create the CinemaDB database
- Create a database user (cinemauser)
- Update appsettings.json with the connection string

## Manual Setup

If the script doesn't work, you can set up manually:

### 1. Set MySQL root password

```bash
sudo mysql

# In MySQL prompt:
ALTER USER 'root'@'localhost' IDENTIFIED BY 'coolpass';
FLUSH PRIVILEGES;
EXIT;
```

### 2. Create database

```bash
mysql -u root -pcoolpass

# In MySQL prompt:
CREATE DATABASE CinemaDB CHARACTER SET utf8mb4 COLLATE utf8mb4_unicode_ci;
EXIT;
```

### 3. Update appsettings.json

Edit `appsettings.json` and change the connection string to:

```json
"DefaultConnection": "Server=localhost;Database=CinemaDB;Uid=root;Pwd=coolpass;CharSet=utf8mb4;"
```

## Run the Application

After setup:

```bash
cd /home/aayush/egui1/CinemaTicketSystemCore
dotnet restore
dotnet build
dotnet run
```

Open browser: `http://localhost:5000`

## Default Login

- Admin: `admin@cinema.com` / `Admin@123`

