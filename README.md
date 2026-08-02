# Astro-Track-Backend

ASP.NET Core backend and REST API for the Astro Track astronomy management platform.

## Continuous Integration

Backend CI runs on pushes to `main` and pull requests targeting `main`.
The workflow restores NuGet dependencies and builds the backend in Release configuration.

Current backend automated test status:

- There is no dedicated backend test project in this repository yet.
- `dotnet test` is intentionally skipped in this workflow for now.
- Adding backend automated tests should be handled in a follow-up issue.

Workflow file:

- `.github/workflows/backend-ci.yml`

## Project baseline

- Framework: ASP.NET Core 8
- Data access: Entity Framework Core with Oracle provider
- Database strategy: Database First
- Source of schema: Astro-Track-Oracle-SQL project
- Important rule: Do not create or run EF Core migrations in this repository

## What has been completed (in order)

### 1) Issue 14: Verify Oracle database connectivity and add database health check

Implemented:

- Added Oracle database health check implementation
- Registered health checks in Program.cs
- Added GET /health endpoint
- Added GET /health/database endpoint
- Verified Oracle connectivity and CELESTIALOBJECTS table query
- Updated local setup documentation
- Added local environment files to .gitignore

Verification completed:

- dotnet restore succeeded
- dotnet build succeeded with 0 warnings and 0 errors
- GET /health returned HTTP 200 with healthy status
- GET /health/database returned HTTP 200 with healthy status
- Oracle reachable at 127.0.0.1:1522/FREEPDB1
- CELESTIALOBJECTS query completed successfully

### 2) Repository layer Step 1: CelestialObject repository

Implemented:

- Added AstroTrack.Api/Repositories/ICelestialObjectRepository.cs
- Added AstroTrack.Api/Repositories/CelestialObjectRepository.cs

Methods added:

- Task<IEnumerable<CelestialObject>> GetAllAsync()
- Task<CelestialObject?> GetByIdAsync(long id)

Repository behavior:

- Uses AstroTrackDbContext
- Uses AsNoTracking() for read-only queries
- Orders list results by ObjectId

Scope of this step:

- Repository interface and implementation only
- Dependency injection registration in Program.cs is deferred to the next step

### 3) Issue 15: Read-only Celestial Objects API endpoints

Implemented:

- Added service layer for read-only Celestial Objects queries
- Added read-only controller endpoints for list and by-id retrieval
- Registered repository and service dependencies in Program.cs

Read-only API endpoints:

- GET /api/celestial-objects
- GET /api/celestial-objects/{id:long}

Example local URLs:

- http://localhost:5000/api/celestial-objects
- http://localhost:5000/api/celestial-objects/1

## Local setup

### 1) Start Oracle container

```powershell
docker ps -a
docker start astrotrack-oracle
docker ps
```

The Oracle Docker container must be running before the API is started. Confirm the container is running and local port 1522 maps to Oracle port 1521.

### 2) Set development environment

```powershell
set ASPNETCORE_ENVIRONMENT=Development
echo %ASPNETCORE_ENVIRONMENT%
```

This must be set to Development so .NET User Secrets are loaded for local credentials.

Expected output:

Development

### 3) Configure OracleDb connection locally

Use .NET User Secrets for local credentials:

```powershell
dotnet user-secrets init
dotnet user-secrets set "ConnectionStrings:OracleDb" "User Id=YOUR_USER;Password=YOUR_PASSWORD;Data Source=127.0.0.1:1522/FREEPDB1;"
```

Credentials must not be committed to source control.

### 4) Prepare schema

Run the Astro Track SQL schema script from the sibling repository before running the backend:

- Astro-Track-Oracle-SQL/sql/Astro_Track_Project.sql

Use additional local SQL scripts only if your Oracle environment requires them.

## Build and run

```powershell
dotnet restore
dotnet build
dotnet run
```

## Health check verification

After the API starts, verify:

- http://localhost:5000/health
- http://localhost:5000/health/database

Example successful response for database health:

```json
{
  "status": "healthy",
  "database": "oracle",
  "check": "CELESTIALOBJECTS"
}
```

## Notes

- This repository is Database First
- EF Core migrations are intentionally not used
- Backend changes do not create or modify Oracle schema objects