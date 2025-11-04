# Admin Account Setup Guide

## Default Admin Account

On first run, the system automatically creates a default admin account:

- **Email:** `admin@cinema.com`
- **Password:** `Admin@123`

This account is created automatically when you first run the application.

## Method 1: Using Default Admin (First Run)

1. Run the application:
   ```bash
   cd /home/aayush/egui1/CinemaTicketSystemCore
   dotnet run
   ```

2. Login with:
   - Email: `admin@cinema.com`
   - Password: `Admin@123`

3. You'll have full admin access!

## Method 2: Make Existing User an Admin (SQL)

If you want to make an existing user an admin:

```bash
mysql -u root -pcoolpass CinemaDB
```

Then run these SQL commands:

```sql
-- Get the user's ID (replace 'user@example.com' with actual email)
SELECT Id FROM AspNetUsers WHERE Email = 'user@example.com';

-- Get the Admin role ID
SELECT Id FROM AspNetRoles WHERE Name = 'Admin';

-- If Admin role doesn't exist, create it:
INSERT INTO AspNetRoles (Id, Name, NormalizedName, ConcurrencyStamp)
VALUES (UUID(), 'Admin', 'ADMIN', UUID());

-- Add user to Admin role (replace USER_ID and ROLE_ID with actual IDs)
INSERT INTO AspNetUserRoles (UserId, RoleId)
VALUES ('USER_ID_FROM_ABOVE', 'ROLE_ID_FROM_ABOVE');

-- Verify
SELECT u.Email, r.Name as Role
FROM AspNetUsers u
JOIN AspNetUserRoles ur ON u.Id = ur.UserId
JOIN AspNetRoles r ON ur.RoleId = r.Id
WHERE u.Email = 'user@example.com';
```

## Method 3: Using the Script

I've created a helper script:

```bash
cd /home/aayush/egui1/CinemaTicketSystemCore
chmod +x scripts/create-admin.sh
./scripts/create-admin.sh user@example.com
```

This automatically:
- Finds the user in the database
- Creates Admin role if it doesn't exist
- Assigns the user to Admin role

## Method 4: Create New Admin User

### Step 1: Register a new user through the web interface

1. Run the application
2. Go to Register page
3. Register with your desired admin email/password

### Step 2: Make that user an admin

Use Method 2 (SQL) or Method 3 (script) to assign admin role.

## Verify Admin Status

Check if a user is admin:

```bash
mysql -u root -pcoolpass CinemaDB -e "
SELECT u.Email, r.Name as Role
FROM AspNetUsers u
LEFT JOIN AspNetUserRoles ur ON u.Id = ur.UserId
LEFT JOIN AspNetRoles r ON ur.RoleId = r.Id
WHERE u.Email = 'user@example.com';
"
```

## Remove Admin Role

To remove admin role from a user:

```sql
DELETE FROM AspNetUserRoles
WHERE UserId = (SELECT Id FROM AspNetUsers WHERE Email = 'user@example.com')
AND RoleId = (SELECT Id FROM AspNetRoles WHERE Name = 'Admin');
```

## Change Admin Password

To change the default admin password:

1. Login as admin
2. Go to "My Profile"
3. Change password (if password change is implemented)

Or via SQL:

```sql
-- Note: Password must be hashed. Better to use the web interface.
-- Or use ASP.NET Identity password reset feature
```

## Quick Reference

**Default Admin Login:**
- Email: `admin@cinema.com`
- Password: `Admin@123`

**Make User Admin (Quick):**
```bash
./scripts/create-admin.sh user@example.com
```

**Check All Admins:**
```bash
mysql -u root -pcoolpass CinemaDB -e "
SELECT u.Email, u.Name, u.Surname
FROM AspNetUsers u
JOIN AspNetUserRoles ur ON u.Id = ur.UserId
JOIN AspNetRoles r ON ur.RoleId = r.Id
WHERE r.Name = 'Admin';
"
```

