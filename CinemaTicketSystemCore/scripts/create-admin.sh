#!/bin/bash

# Script to create or assign admin role to a user
# Usage: ./create-admin.sh [email]

set -e

MYSQL_PASSWORD="coolpass"
DB_NAME="CinemaDB"

if [ -z "$1" ]; then
    echo "Usage: ./create-admin.sh [user-email]"
    echo ""
    echo "Example: ./create-admin.sh admin@cinema.com"
    echo "Example: ./create-admin.sh user@example.com"
    echo ""
    echo "Note: The user must already exist in the database."
    echo "If you want to create a new admin user, first register through the web interface."
    exit 1
fi

USER_EMAIL="$1"

echo "Making user '$USER_EMAIL' an admin..."
echo ""

# Get user ID and role ID from database
RESULT=$(mysql -u root -p"${MYSQL_PASSWORD}" "${DB_NAME}" -e "
SELECT u.Id 
FROM AspNetUsers u 
WHERE u.Email = '${USER_EMAIL}';
" 2>/dev/null | tail -n 1)

if [ -z "$RESULT" ]; then
    echo "Error: User '$USER_EMAIL' not found in database."
    echo "Please register the user first through the web interface."
    exit 1
fi

USER_ID=$(echo "$RESULT" | tr -d ' ')

# Check if Admin role exists
ROLE_ID=$(mysql -u root -p"${MYSQL_PASSWORD}" "${DB_NAME}" -e "
SELECT Id 
FROM AspNetRoles 
WHERE Name = 'Admin';
" 2>/dev/null | tail -n 1)

if [ -z "$ROLE_ID" ]; then
    echo "Creating Admin role..."
    ROLE_ID=$(mysql -u root -p"${MYSQL_PASSWORD}" "${DB_NAME}" -e "
    INSERT INTO AspNetRoles (Id, Name, NormalizedName, ConcurrencyStamp)
    VALUES (UUID(), 'Admin', 'ADMIN', UUID());
    SELECT Id FROM AspNetRoles WHERE Name = 'Admin';
    " 2>/dev/null | tail -n 1)
fi

ROLE_ID=$(echo "$ROLE_ID" | tr -d ' ')

# Check if user already has admin role
EXISTING=$(mysql -u root -p"${MYSQL_PASSWORD}" "${DB_NAME}" -e "
SELECT COUNT(*) 
FROM AspNetUserRoles 
WHERE UserId = '${USER_ID}' AND RoleId = '${ROLE_ID}';
" 2>/dev/null | tail -n 1)

if [ "$EXISTING" = "1" ]; then
    echo "User '$USER_EMAIL' is already an admin."
    exit 0
fi

# Add user to admin role
mysql -u root -p"${MYSQL_PASSWORD}" "${DB_NAME}" -e "
INSERT INTO AspNetUserRoles (UserId, RoleId)
VALUES ('${USER_ID}', '${ROLE_ID}');
" 2>/dev/null

echo "✓ User '$USER_EMAIL' is now an admin!"
echo ""
echo "You can now log in and access admin features."

