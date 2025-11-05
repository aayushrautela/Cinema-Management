#!/bin/bash

# Simple MySQL Setup Script
# Sets MySQL root password to 'coolpass' and creates database

MYSQL_PASSWORD="coolpass"
DB_NAME="CinemaDB"

echo "Setting up MySQL..."

# Install MySQL if not installed
echo "Installing MySQL server..."
sudo dnf install -y mysql-server

# Start MySQL service
echo "Starting MySQL service..."
sudo systemctl start mysqld
sudo systemctl enable mysqld

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
if mysql -u root -p"${MYSQL_PASSWORD}" <<EOF 2>/dev/null
CREATE DATABASE IF NOT EXISTS ${DB_NAME} CHARACTER SET utf8mb4 COLLATE utf8mb4_unicode_ci;
EOF
then
    echo "Database created successfully"
else
    sudo mysql <<EOF
CREATE DATABASE IF NOT EXISTS ${DB_NAME} CHARACTER SET utf8mb4 COLLATE utf8mb4_unicode_ci;
EOF
fi

# Create database user
echo "Creating database user..."
if mysql -u root -p"${MYSQL_PASSWORD}" <<EOF 2>/dev/null
CREATE USER IF NOT EXISTS 'cinemauser'@'localhost' IDENTIFIED BY '${MYSQL_PASSWORD}';
GRANT ALL PRIVILEGES ON ${DB_NAME}.* TO 'cinemauser'@'localhost';
FLUSH PRIVILEGES;
EOF
then
    echo "User created successfully"
else
    sudo mysql <<EOF
CREATE USER IF NOT EXISTS 'cinemauser'@'localhost' IDENTIFIED BY '${MYSQL_PASSWORD}';
GRANT ALL PRIVILEGES ON ${DB_NAME}.* TO 'cinemauser'@'localhost';
FLUSH PRIVILEGES;
EOF
fi

# Update appsettings.json
echo "Updating appsettings.json..."
sed -i "s|\"Pwd=.*\"|\"Pwd=${MYSQL_PASSWORD}\"|" appsettings.json

echo "✓ Setup complete!"
echo "✓ MySQL root password: ${MYSQL_PASSWORD}"
echo "✓ Database: ${DB_NAME}"
echo ""
echo "You can now run: dotnet run"

