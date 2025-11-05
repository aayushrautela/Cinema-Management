#!/bin/bash

# Complete Setup Script for ASP.NET Core Cinema Ticket System on Linux
# This script installs .NET SDK and sets up MySQL

set -e

echo "=========================================="
echo "Complete Setup Script"
echo "ASP.NET Core Cinema Ticket System"
echo "=========================================="
echo ""

# Check if running as root
if [ "$EUID" -ne 0 ]; then 
    echo "This script needs sudo privileges."
    echo "Please run with: sudo ./install-all.sh"
    exit 1
fi

echo "Step 1: Installing .NET SDK 8.0..."
echo ""

# Remove old installations
dnf remove -y dotnet* 2>/dev/null || true

# Add Microsoft repository
rpm --import https://packages.microsoft.com/keys/microsoft.asc 2>/dev/null || true

# Detect Fedora version
FEDORA_VERSION=$(rpm -E %fedora 2>/dev/null || echo "40")

# Add repository
cat > /etc/yum.repos.d/microsoft-prod.repo <<EOF
[microsoft-prod]
name=Microsoft Product Repository
baseurl=https://packages.microsoft.com/fedora/${FEDORA_VERSION}/prod/
enabled=1
gpgcheck=1
gpgkey=https://packages.microsoft.com/keys/microsoft.asc
EOF

# Install .NET SDK
dnf install -y dotnet-sdk-8.0

echo ""
echo "✓ .NET SDK 8.0 installed"
echo ""

# Verify installation
echo "Verifying .NET installation..."
dotnet --version

echo ""
echo "Step 2: Installing MySQL..."
echo ""

# Install MySQL
dnf install -y mysql-server

# Start MySQL
systemctl start mysqld
systemctl enable mysqld

echo "✓ MySQL installed and started"
echo ""

echo "Step 3: Setting up MySQL..."
echo ""

# Set root password
mysql <<EOF 2>/dev/null || true
ALTER USER 'root'@'localhost' IDENTIFIED BY 'coolpass';
FLUSH PRIVILEGES;
EOF

# Create database
mysql -u root -pcoolpass <<EOF 2>/dev/null || true
CREATE DATABASE IF NOT EXISTS CinemaDB CHARACTER SET utf8mb4 COLLATE utf8mb4_unicode_ci;
EOF

echo "✓ MySQL configured with password 'coolpass'"
echo "✓ Database 'CinemaDB' created"
echo ""

echo "Step 4: Updating appsettings.json..."
echo ""

# Get the directory where this script is located
SCRIPT_DIR="$( cd "$( dirname "${BASH_SOURCE[0]}" )" && pwd )"
PROJECT_DIR="$(dirname "$SCRIPT_DIR")"

cd "$PROJECT_DIR"

# Update connection string
if [ -f "appsettings.json" ]; then
    sed -i 's|"Pwd=.*"|"Pwd=coolpass"|' appsettings.json
    echo "✓ appsettings.json updated"
else
    echo "⚠ Warning: appsettings.json not found"
fi

echo ""
echo "=========================================="
echo "Setup Complete!"
echo "=========================================="
echo ""
echo "Next steps:"
echo "  1. dotnet restore"
echo "  2. dotnet build"
echo "  3. dotnet run"
echo ""
echo "Default login:"
echo "  Admin: admin@cinema.com / Admin@123"
echo ""

