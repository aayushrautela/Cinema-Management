# Setup Guide for ASP.NET Core on Linux (Fedora)

## Step 1: Install .NET SDK 8.0

### Option A: Using DNF (Recommended)

```bash
# Remove any old/corrupted installations
sudo dnf remove -y dotnet* 2>/dev/null || true

# Add Microsoft repository
sudo rpm --import https://packages.microsoft.com/keys/microsoft.asc

# Add repository for Fedora 40 (adjust version if needed)
sudo dnf config-manager --add-repo https://packages.microsoft.com/fedora/40/prod/

# Install .NET SDK 8.0
sudo dnf install -y dotnet-sdk-8.0

# Verify installation
dotnet --version
```

### Option B: Using Microsoft Installer Script

```bash
# Download installer
cd /tmp
wget https://dot.net/v1/dotnet-install.sh
chmod +x dotnet-install.sh

# Install to system directory
sudo ./dotnet-install.sh --channel 8.0 --install-dir /usr/share/dotnet

# Create symlink
sudo ln -sf /usr/share/dotnet/dotnet /usr/bin/dotnet

# Verify
dotnet --version
```

### Option C: User Installation (No sudo)

```bash
# Download installer
cd /tmp
wget https://dot.net/v1/dotnet-install.sh
chmod +x dotnet-install.sh

# Install to user directory
./dotnet-install.sh --channel 8.0 --install-dir ~/.dotnet

# Add to PATH (add this to ~/.bashrc)
echo 'export PATH=$PATH:$HOME/.dotnet' >> ~/.bashrc
source ~/.bashrc

# Verify
dotnet --version
```

## Step 2: Setup MySQL

```bash
# Install MySQL (if not installed)
sudo dnf install -y mysql-server

# Start MySQL
sudo systemctl start mysqld
sudo systemctl enable mysqld

# Run setup script
cd /home/aayush/egui1/CinemaTicketSystemCore
./scripts/setup-mysql.sh
```

Or manually:

```bash
# Set MySQL root password
sudo mysql
ALTER USER 'root'@'localhost' IDENTIFIED BY 'coolpass';
FLUSH PRIVILEGES;
EXIT;

# Create database
mysql -u root -pcoolpass
CREATE DATABASE CinemaDB CHARACTER SET utf8mb4 COLLATE utf8mb4_unicode_ci;
EXIT;
```

## Step 3: Update Connection String

Edit `appsettings.json`:

```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Server=localhost;Database=CinemaDB;Uid=root;Pwd=coolpass;CharSet=utf8mb4;"
  }
}
```

## Step 4: Run the Application

```bash
cd /home/aayush/egui1/CinemaTicketSystemCore
dotnet restore
dotnet build
dotnet run
```

## Troubleshooting

### .NET SDK Not Found

```bash
# Check if installed
which dotnet
dotnet --version

# If not found, check PATH
echo $PATH

# Reinstall using Option A above
```

### MySQL Connection Error

```bash
# Check MySQL status
sudo systemctl status mysqld

# Test connection
mysql -u root -pcoolpass -e "SHOW DATABASES;"
```

### Build Errors

```bash
# Clean and rebuild
dotnet clean
dotnet restore
dotnet build
```

## Quick Commands

```bash
# Full setup (run all commands)
sudo dnf install -y dotnet-sdk-8.0 mysql-server
sudo systemctl start mysqld
cd /home/aayush/egui1/CinemaTicketSystemCore
./scripts/setup-mysql.sh
dotnet restore
dotnet build
dotnet run
```

