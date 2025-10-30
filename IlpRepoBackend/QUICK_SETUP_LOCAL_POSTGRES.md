# Quick Setup - Local PostgreSQL

## 1?? Install PostgreSQL

**Download:** https://www.postgresql.org/download/

**Default Settings:**
- Port: `5432`
- Username: `postgres`
- Remember your password!

## 2?? Create Database

**Option A - pgAdmin (GUI):**
- Open pgAdmin
- Right-click Databases ? Create ? Database
- Name: `ilp_repo_db`

**Option B - Command Line:**
```bash
psql -U postgres
CREATE DATABASE ilp_repo_db;
\q
```

## 3?? Update Password in Config

Edit `IlpRepoBackend.Api\appsettings.Development.json`:

```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Host=localhost;Port=5432;Database=ilp_repo_db;Username=postgres;Password=PUT_YOUR_PASSWORD_HERE"
  }
}
```

Replace `PUT_YOUR_PASSWORD_HERE` with your actual PostgreSQL password.

## 4?? Run Migrations

```bash
cd IlpRepoBackend.Infrastructure
dotnet ef database update --startup-project ../IlpRepoBackend.Api
```

## 5?? Run Application

```bash
cd IlpRepoBackend.Api
dotnet run
```

Open: `https://localhost:5001/swagger`

## ? Done!

Your app is now using **local PostgreSQL** instead of Supabase.

---

## Troubleshooting

**Can't connect?**
- Check PostgreSQL is running (Services on Windows)
- Verify password is correct
- Ensure port 5432 is not blocked

**Tables not created?**
```bash
dotnet ef database update --startup-project ../IlpRepoBackend.Api
```

**Need to reset?**
```bash
# Drop database
psql -U postgres
DROP DATABASE ilp_repo_db;
CREATE DATABASE ilp_repo_db;
\q

# Run migrations again
dotnet ef database update --startup-project ../IlpRepoBackend.Api
```
