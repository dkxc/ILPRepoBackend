# Local PostgreSQL Setup Guide

## Overview

Your application is now configured to use a **local PostgreSQL database** instead of Supabase.

## Prerequisites

### 1. Install PostgreSQL

**Windows:**
- Download from: https://www.postgresql.org/download/windows/
- Run the installer
- Remember the password you set for the `postgres` user
- Default port: 5432

**macOS:**
```bash
# Using Homebrew
brew install postgresql@15
brew services start postgresql@15
```

**Linux (Ubuntu/Debian):**
```bash
sudo apt update
sudo apt install postgresql postgresql-contrib
sudo systemctl start postgresql
sudo systemctl enable postgresql
```

### 2. Verify PostgreSQL is Running

**Windows:**
- Open Services (Win + R ? services.msc)
- Look for "postgresql-x64-15" or similar
- Status should be "Running"

**macOS/Linux:**
```bash
# Check status
pg_isready

# Or
sudo systemctl status postgresql
```

## Database Setup

### Step 1: Create Database

**Option A: Using pgAdmin (GUI)**
1. Open pgAdmin 4
2. Connect to PostgreSQL Server (localhost)
3. Right-click on "Databases" ? Create ? Database
4. Name: `ilp_repo_db`
5. Owner: `postgres`
6. Click "Save"

**Option B: Using Command Line (psql)**

**Windows:**
```cmd
# Open Command Prompt or PowerShell
psql -U postgres

# In psql shell:
CREATE DATABASE ilp_repo_db;
\q
```

**macOS/Linux:**
```bash
# Switch to postgres user (Linux)
sudo -u postgres psql

# Or connect directly (macOS)
psql postgres

# Create database
CREATE DATABASE ilp_repo_db;
\q
```

### Step 2: Update Connection String

Open `appsettings.json` or `appsettings.Development.json` and update:

```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Host=localhost;Port=5432;Database=ilp_repo_db;Username=postgres;Password=YOUR_ACTUAL_PASSWORD"
  }
}
```

**Connection String Parameters:**
- `Host=localhost` - Your local machine
- `Port=5432` - Default PostgreSQL port
- `Database=ilp_repo_db` - Database name
- `Username=postgres` - Default admin user
- `Password=...` - The password you set during PostgreSQL installation

### Step 3: Run Migrations

```bash
# Navigate to Infrastructure project
cd IlpRepoBackend.Infrastructure

# Create initial migration (if not already done)
dotnet ef migrations add InitialCreate --startup-project ../IlpRepoBackend.Api

# Apply migrations to create tables
dotnet ef database update --startup-project ../IlpRepoBackend.Api
```

### Step 4: Verify Database Creation

**Using pgAdmin:**
1. Refresh the database list
2. Expand `ilp_repo_db` ? Schemas ? public ? Tables
3. You should see all your tables created

**Using psql:**
```bash
psql -U postgres -d ilp_repo_db

# List all tables
\dt

# View table structure
\d users
\d projects
\d document_submissions

# Exit
\q
```

## Connection String Variations

### Standard Connection (Development)
```json
"DefaultConnection": "Host=localhost;Port=5432;Database=ilp_repo_db;Username=postgres;Password=yourpassword"
```

### With Connection Pooling
```json
"DefaultConnection": "Host=localhost;Port=5432;Database=ilp_repo_db;Username=postgres;Password=yourpassword;Pooling=true;MinPoolSize=1;MaxPoolSize=20"
```

### With Timeout Settings
```json
"DefaultConnection": "Host=localhost;Port=5432;Database=ilp_repo_db;Username=postgres;Password=yourpassword;Timeout=30;CommandTimeout=30"
```

### For Production (with SSL)
```json
"DefaultConnection": "Host=localhost;Port=5432;Database=ilp_repo_db;Username=postgres;Password=yourpassword;SSL Mode=Require;Trust Server Certificate=true"
```

## Security Best Practices

### 1. Use User Secrets (Development)

Instead of storing the password in `appsettings.json`, use User Secrets:

```bash
# Navigate to API project
cd IlpRepoBackend.Api

# Initialize user secrets
dotnet user-secrets init

# Set connection string
dotnet user-secrets set "ConnectionStrings:DefaultConnection" "Host=localhost;Port=5432;Database=ilp_repo_db;Username=postgres;Password=yourpassword"
```

Then remove the connection string from `appsettings.json`.

### 2. Use Environment Variables (Production)

Set environment variable:

**Windows:**
```cmd
setx ConnectionStrings__DefaultConnection "Host=localhost;Port=5432;Database=ilp_repo_db;Username=postgres;Password=yourpassword"
```

**Linux/macOS:**
```bash
export ConnectionStrings__DefaultConnection="Host=localhost;Port=5432;Database=ilp_repo_db;Username=postgres;Password=yourpassword"
```

### 3. Create Application-Specific User

Instead of using `postgres` superuser:

```sql
-- Connect as postgres user
psql -U postgres

-- Create application user
CREATE USER ilp_app WITH PASSWORD 'your_secure_password';

-- Grant privileges
GRANT ALL PRIVILEGES ON DATABASE ilp_repo_db TO ilp_app;

-- Connect to the database
\c ilp_repo_db

-- Grant schema privileges
GRANT ALL ON SCHEMA public TO ilp_app;
GRANT ALL PRIVILEGES ON ALL TABLES IN SCHEMA public TO ilp_app;
GRANT ALL PRIVILEGES ON ALL SEQUENCES IN SCHEMA public TO ilp_app;
```

Then update connection string:
```json
"DefaultConnection": "Host=localhost;Port=5432;Database=ilp_repo_db;Username=ilp_app;Password=your_secure_password"
```

## Troubleshooting

### Error: "Could not connect to server"

**Check 1: PostgreSQL is running**
```bash
# Windows
services.msc (look for postgresql service)

# macOS/Linux
sudo systemctl status postgresql
```

**Check 2: Port is correct**
```bash
# Check PostgreSQL port
psql -U postgres -c "SHOW port;"
```

**Check 3: Firewall settings**
- Ensure port 5432 is not blocked
- Add firewall rule if needed

### Error: "password authentication failed"

- Verify password is correct
- Check `pg_hba.conf` file for authentication settings
- Try resetting postgres password:

```bash
# Linux
sudo -u postgres psql
ALTER USER postgres PASSWORD 'new_password';

# Windows (in psql)
psql -U postgres
ALTER USER postgres PASSWORD 'new_password';
```

### Error: "database does not exist"

```bash
# Create the database
psql -U postgres
CREATE DATABASE ilp_repo_db;
\q
```

### Error: "relation does not exist"

- Run migrations:
```bash
cd IlpRepoBackend.Infrastructure
dotnet ef database update --startup-project ../IlpRepoBackend.Api
```

## Useful PostgreSQL Commands

```sql
-- List all databases
\l

-- Connect to database
\c ilp_repo_db

-- List all tables
\dt

-- Describe table structure
\d table_name

-- View table data
SELECT * FROM users LIMIT 10;

-- Check database size
SELECT pg_size_pretty(pg_database_size('ilp_repo_db'));

-- List all connections
SELECT * FROM pg_stat_activity WHERE datname = 'ilp_repo_db';

-- Kill connections (if needed for migration)
SELECT pg_terminate_backend(pid) 
FROM pg_stat_activity 
WHERE datname = 'ilp_repo_db' AND pid <> pg_backend_pid();
```

## Comparing Supabase vs Local PostgreSQL

### Supabase (Cloud - Before)
```json
"DefaultConnection": "Server=aws-1-ap-south-1.pooler.supabase.com;Port=6543;Database=postgres;User Id=postgres.jbbaufdzfgglkjqiwveb;Password=BwdsuOT2YH4G1sL9;SSL Mode=Require;Trust Server Certificate=true;Pooling=false"
```

**Pros:**
- ? Hosted/Managed
- ? Automatic backups
- ? Built-in features (Auth, Storage, etc.)

**Cons:**
- ? Requires internet connection
- ? Potential latency
- ? Costs for production

### Local PostgreSQL (Now)
```json
"DefaultConnection": "Host=localhost;Port=5432;Database=ilp_repo_db;Username=postgres;Password=yourpassword"
```

**Pros:**
- ? No internet required
- ? Faster (local connection)
- ? Free
- ? Full control
- ? Better for development

**Cons:**
- ? Manual backups
- ? You manage everything
- ? Need to install PostgreSQL

## Migration Checklist

- [x] Update `appsettings.json` connection string
- [x] Update `appsettings.Development.json` connection string
- [ ] Install PostgreSQL locally
- [ ] Create `ilp_repo_db` database
- [ ] Update connection string with your password
- [ ] Run migrations: `dotnet ef database update`
- [ ] Test API connection
- [ ] Verify tables were created
- [ ] (Optional) Set up user secrets for password
- [ ] (Optional) Create application-specific database user

## Next Steps

1. **Install PostgreSQL** (if not already installed)
2. **Create the database** using pgAdmin or psql
3. **Update the password** in connection string
4. **Run migrations** to create tables
5. **Test the application** - Run the API and check Swagger
6. **Verify data** - Create some test records

## Running the Application

```bash
# Navigate to API project
cd IlpRepoBackend.Api

# Run the application
dotnet run

# Or with hot reload
dotnet watch run
```

Access Swagger: `https://localhost:5001/swagger`

## Database Backup (Recommended)

**Backup:**
```bash
pg_dump -U postgres -d ilp_repo_db -F c -b -v -f ilp_repo_backup.dump
```

**Restore:**
```bash
pg_restore -U postgres -d ilp_repo_db -v ilp_repo_backup.dump
```

## Summary

Your application is now configured to use **local PostgreSQL**!

**What changed:**
- Connection strings updated to `localhost`
- Removed Supabase-specific SSL settings
- Standard PostgreSQL connection parameters

**What stayed the same:**
- Still using PostgreSQL (same database type)
- Same `UseNpgsql` provider
- Same database schema
- All your existing code works without changes!

The only difference is WHERE the database is hosted (local machine vs Supabase cloud).
