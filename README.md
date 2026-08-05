# Astro Track Backend

ASP.NET Core backend API for the Astro Track astronomy management platform.

## Live Production Status

- Backend API: https://astrotrack-api.jollymeadow-cbeb8eb6.canadacentral.azurecontainerapps.io
- Health endpoint: https://astrotrack-api.jollymeadow-cbeb8eb6.canadacentral.azurecontainerapps.io/health
- Database health endpoint: https://astrotrack-api.jollymeadow-cbeb8eb6.canadacentral.azurecontainerapps.io/health/database
- Frontend (Azure Static Web Apps): https://thankful-desert-046da3c0f.7.azurestaticapps.net

## Related Repositories

- Frontend: https://github.com/Jaturaput-Jongsubcharoen/Astro-Track-Frontend
- Backend: https://github.com/Jaturaput-Jongsubcharoen/Astro-Track-Backend
- Oracle SQL: https://github.com/Jaturaput-Jongsubcharoen/Astro-Track-Oracle-SQL

## Technology and Architecture

- Framework: ASP.NET Core 8
- Data access: Entity Framework Core with Oracle provider
- Database strategy: Database First
- API style: REST with controller-based resources
- Layering: Controller -> Service -> Repository -> DbContext/Entity Models
- Error handling: ProblemDetails + production exception handler
- Hosting target: Azure Container Apps

Important rule:

- Do not create or run EF Core migrations in this repository.

## Implemented API Surface

### Health

- GET /health
- GET /health/database
- GET /api/health

### Celestial Objects

- GET /api/celestial-objects
- GET /api/celestial-objects/{id}
- POST /api/celestial-objects
- PUT /api/celestial-objects/{id}
- DELETE /api/celestial-objects/{id}

### Researchers

- GET /api/researchers
- GET /api/researchers/{id}
- POST /api/researchers
- PUT /api/researchers/{id}
- DELETE /api/researchers/{id}

### Missions

- GET /api/missions
- GET /api/missions/{id}
- POST /api/missions
- PUT /api/missions/{id}
- DELETE /api/missions/{id}

### Observations

- GET /api/observations
- GET /api/observations/{id}
- POST /api/observations
- PUT /api/observations/{id}
- DELETE /api/observations/{id}

## Health Checks

- /health is an application liveness endpoint.
- /health/database verifies Oracle connectivity and CELESTIALOBJECTS table queryability.

Example healthy database response:

```json
{
  "status": "healthy",
  "database": "oracle",
  "check": "CELESTIALOBJECTS"
}
```

## Runtime Configuration

Configuration sources:

- appsettings files
- environment variables
- .NET User Secrets (local development)

Required in production:

- ConnectionStrings__OracleDb
- AllowedOrigins__0 (and optional AllowedOrigins__1, AllowedOrigins__2, ...)
- ASPNETCORE_ENVIRONMENT=Production
- ASPNETCORE_URLS=http://+:5000

### CORS Behavior

- Development automatically includes:
  - http://localhost:4200
  - https://localhost:4200
- Production uses configured AllowedOrigins values only.
- Wildcard CORS is intentionally not enabled.

Example production CORS origin:

```powershell
set AllowedOrigins__0=https://thankful-desert-046da3c0f.7.azurestaticapps.net
```

Use origin values without a trailing slash.

### Oracle Connection Configuration

- Do not place Oracle credentials in appsettings files.
- Local development should use User Secrets.
- Container and cloud hosting should inject ConnectionStrings__OracleDb via environment variables.

Example local secret:

```powershell
dotnet user-secrets init
dotnet user-secrets set "ConnectionStrings:OracleDb" "User Id=YOUR_USER;Password=YOUR_PASSWORD;Data Source=127.0.0.1:1522/FREEPDB1;"
```

### Reverse Proxy and HTTPS

- Forwarded headers are enabled for reverse-proxy hosting.
- HTTPS redirection is enabled.
- Production exception responses return generic ProblemDetails while full details remain in server logs.

## Local Development Setup

### 1. Start Oracle

```powershell
docker ps -a
docker start astrotrack-oracle
docker ps
```

The Oracle container must be running before API startup.

### 2. Set environment to Development

```powershell
set ASPNETCORE_ENVIRONMENT=Development
echo %ASPNETCORE_ENVIRONMENT%
```

### 3. Configure OracleDb secret

```powershell
dotnet user-secrets init
dotnet user-secrets set "ConnectionStrings:OracleDb" "User Id=YOUR_USER;Password=YOUR_PASSWORD;Data Source=127.0.0.1:1522/FREEPDB1;"
```

### 4. Prepare schema

Run the SQL schema from the Oracle SQL repository before running backend locally:

- Astro-Track-Oracle-SQL/sql/Astro_Track_Project.sql

## Build, Run, and Test

Run locally:

```powershell
dotnet restore
dotnet build
dotnet run
```

Release verification:

```powershell
dotnet restore
dotnet build --configuration Release --no-restore
dotnet test --configuration Release --no-build
```

Health verification:

```powershell
curl http://localhost:5000/health
curl http://localhost:5000/health/database
```

## Docker Operations

This repository includes a production Dockerfile at repo root.

Build image:

```powershell
docker build -t astro-track-backend:prod .
```

Run image locally (example):

```powershell
docker run --rm -p 5001:5000 ^
  -e ASPNETCORE_ENVIRONMENT=Production ^
  -e ASPNETCORE_URLS=http://+:5000 ^
  -e ConnectionStrings__OracleDb="User Id=YOUR_USER;Password=YOUR_PASSWORD;Data Source=host.docker.internal:1522/FREEPDB1;" ^
  -e AllowedOrigins__0=http://localhost:4200 ^
  astro-track-backend:prod
```

Note:

- Local multi-container orchestration (frontend + backend + oracle) is maintained from the frontend repository Docker Compose workflow.

## Azure Container Apps Deployment Notes

Set application configuration in Azure environment variables/secrets:

- ConnectionStrings__OracleDb (secret)
- AllowedOrigins__0=https://thankful-desert-046da3c0f.7.azurestaticapps.net
- ASPNETCORE_ENVIRONMENT=Production
- ASPNETCORE_URLS=http://+:5000

When you update environment variables in Azure Container Apps, a new revision is created.

## CI Workflow

GitHub Actions workflow:

- .github/workflows/backend-ci.yml

Pipeline steps:

- dotnet restore
- dotnet build --configuration Release --no-restore
- dotnet test --configuration Release --no-build

## Current Test Coverage

Current unit tests are focused on Celestial Objects:

- AstroTrack.Api.Tests/Services/CelestialObjectServiceTests.cs
- AstroTrack.Api.Tests/Controllers/CelestialObjectsControllerTests.cs

Researchers, Missions, and Observations currently have implementation but do not yet have equivalent unit test suites in this repository.

## Security and Secrets

- Do not commit .env files with real credentials.
- Do not commit Oracle wallet files, key stores, or tokens.
- Keep production secrets in Azure-managed secret configuration.
- Keep local credentials in .NET User Secrets or uncommitted .env files.

## Repository Notes

- Database First is enforced.
- EF Core migrations are intentionally not used.
- Backend code changes do not modify Oracle schema objects.
