#!/bin/bash

# Simple MySQL Setup Script
# Sets MySQL root password to 'coolpass' and creates database

MYSQL_PASSWORD="coolpass"
DB_NAME="CinemaDB"

echo "Setting up MySQL..."

# Start MySQL if not running
sudo systemctl start mysqld 2>/dev/null || true

# Try to set root password (works if MySQL has no password or uses sudo)
echo "Setting MySQL root password to 'coolpass'..."

# Method 1: If MySQL has no password
mysql -u root <<EOF 2>/dev/null || true
ALTER USER 'root'@'localhost' IDENTIFIED BY '${MYSQL_PASSWORD}';
FLUSH PRIVILEGES;
EOF

# Method 2: Using sudo (if method 1 fails)
sudo mysql <<EOF 2>/dev/null || true
ALTER USER 'root'@'localhost' IDENTIFIED BY '${MYSQL_PASSWORD}';
FLUSH PRIVILEGES;
EOF

# Create database
echo "Creating database..."
mysql -u root -p"${MYSQL_PASSWORD}" <<EOF 2>/dev/null || sudo mysql -u root -p"${MYSQL_PASSWORD}" <<EOF
CREATE DATABASE IF NOT EXISTS ${DB_NAME} CHARACTER SET utf8mb4 COLLATE utf8mb4_unicode_ci;
EOF

# Update appsettings.json
echo "Updating appsettings.json..."
sed -i "s|\"Pwd=.*\"|\"Pwd=${MYSQL_PASSWORD}\"|" appsettings.json

echo "✓ Setup complete!"
echo "✓ MySQL root password: ${MYSQL_PASSWORD}"
echo "✓ Database: ${DB_NAME}"
echo ""
echo "You can now run: dotnet run"

