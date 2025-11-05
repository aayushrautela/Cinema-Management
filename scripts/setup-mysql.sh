#!/bin/bash

# MySQL Setup Script for Cinema Ticket System
# This script sets up MySQL with password 'coolpass' and creates the database

set -e

MYSQL_ROOT_PASSWORD="coolpass"
DB_NAME="CinemaDB"
DB_USER="cinemauser"
DB_PASSWORD="coolpass"

echo "=========================================="
echo "MySQL Setup Script for Cinema Ticket System"
echo "=========================================="
echo ""

# Check if MySQL is running
if ! systemctl is-active --quiet mysqld; then
    echo "Starting MySQL service..."
    sudo systemctl start mysqld
    sudo systemctl enable mysqld
    sleep 2
fi

echo "MySQL service is running."
echo ""

# Check if MySQL root password is already set
echo "Attempting to set MySQL root password..."
echo ""

# Try to connect without password first
if mysql -u root -e "SELECT 1" > /dev/null 2>&1; then
    echo "MySQL root has no password. Setting password to 'coolpass'..."
    mysql -u root <<EOF
ALTER USER 'root'@'localhost' IDENTIFIED BY '${MYSQL_ROOT_PASSWORD}';
FLUSH PRIVILEGES;
EOF
    echo "✓ Root password set successfully!"
elif mysql -u root -p"${MYSQL_ROOT_PASSWORD}" -e "SELECT 1" > /dev/null 2>&1; then
    echo "✓ MySQL root password is already 'coolpass'"
else
    echo "⚠ Warning: Could not connect to MySQL. You may need to reset the root password manually."
    echo "Run: sudo mysql_secure_installation"
    exit 1
fi

echo ""
echo "Creating database and user..."
echo ""

# Create database and user
mysql -u root -p"${MYSQL_ROOT_PASSWORD}" <<EOF
-- Create database if not exists
CREATE DATABASE IF NOT EXISTS ${DB_NAME} CHARACTER SET utf8mb4 COLLATE utf8mb4_unicode_ci;

-- Create user if not exists
CREATE USER IF NOT EXISTS '${DB_USER}'@'localhost' IDENTIFIED BY '${DB_PASSWORD}';

-- Grant privileges
GRANT ALL PRIVILEGES ON ${DB_NAME}.* TO '${DB_USER}'@'localhost';

-- Flush privileges
FLUSH PRIVILEGES;

-- Show created databases
SHOW DATABASES LIKE '${DB_NAME}';
EOF

echo ""
echo "✓ Database '${DB_NAME}' created successfully!"
echo "✓ User '${DB_USER}' created with privileges!"
echo ""

# Update appsettings.json
echo "Updating appsettings.json with database connection..."
echo ""

CONFIG_FILE="appsettings.json"

if [ -f "$CONFIG_FILE" ]; then
    # Create backup
    cp "$CONFIG_FILE" "${CONFIG_FILE}.backup"
    echo "✓ Created backup: ${CONFIG_FILE}.backup"
    
    # Update connection string using sed
    sed -i "s|\"DefaultConnection\": \".*\"|\"DefaultConnection\": \"Server=localhost;Database=${DB_NAME};Uid=${DB_USER};Pwd=${DB_PASSWORD};CharSet=utf8mb4;\"|" "$CONFIG_FILE"
    
    echo "✓ Updated appsettings.json with connection string"
    echo ""
    echo "Connection string updated to:"
    echo "Server=localhost;Database=${DB_NAME};Uid=${DB_USER};Pwd=${DB_PASSWORD};CharSet=utf8mb4;"
else
    echo "⚠ Warning: appsettings.json not found. Please update it manually:"
    echo "  \"DefaultConnection\": \"Server=localhost;Database=${DB_NAME};Uid=${DB_USER};Pwd=${DB_PASSWORD};CharSet=utf8mb4;\""
fi

echo ""
echo "=========================================="
echo "Setup Complete!"
echo "=========================================="
echo ""
echo "MySQL Configuration:"
echo "  Root Password: ${MYSQL_ROOT_PASSWORD}"
echo "  Database: ${DB_NAME}"
echo "  Database User: ${DB_USER}"
echo "  Database Password: ${DB_PASSWORD}"
echo ""
echo "Next steps:"
echo "  1. dotnet restore"
echo "  2. dotnet build"
echo "  3. dotnet run"
echo ""
echo "The database will be created automatically on first run!"
echo ""

