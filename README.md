# Astro-Track-Backend

ASP.NET Core backend and REST API for the Astro Track astronomy management platform.

## Continuous Integration

Backend CI runs on pushes to `main` and pull requests targeting `main`.
The workflow restores NuGet dependencies, builds the backend in Release configuration, and runs automated backend unit tests.

Backend test project:

- `AstroTrack.Api.Tests`
- Framework and tooling: xUnit, Moq, Microsoft.NET.Test.Sdk, xunit.runner.visualstudio, coverlet.collector
- Coverage scope: CelestialObjectService unit tests (including full entity-to-DTO mapping) and CelestialObjectsController unit tests
- Local test command: `dotnet test --configuration Release --no-build`
- Unit tests do not require a live Oracle database or Docker

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

### 4) Issue 24: Celestial Objects create, update, and delete APIs

Implemented:

- Added POST, PUT, and DELETE endpoints for Celestial Objects.
- Added request DTO validation aligned with Oracle CELESTIALOBJECTS constraints.
- Added repository and service mutation methods with async EF Core usage.
- Added safe duplicate/constraint handling without exposing raw Oracle errors.
- Added controller and service unit tests for mutation flows.

Mutation endpoints:

- POST /api/celestial-objects
- PUT /api/celestial-objects/{id:long}
- DELETE /api/celestial-objects/{id:long}

ID strategy:

- POST uses client-supplied ObjectId in request body.
- Duplicate ObjectId returns HTTP 409 Conflict.

Example POST request:

```json
{
  "objectId": 12001,
  "objectName": "Sample Exoplanet",
  "category": "Exoplanet",
  "distanceLightYears": 42.123456,
  "discoveryDate": "2026-01-15T00:00:00Z",
  "inSolarSystem": "N",
  "habitabilityScore": 7.25,
  "surfaceTemperature": -12.3,
  "gravity": 1.08,
  "nitrogen": "Y",
  "oxygen": "Y",
  "co2": "N",
  "sulfuricAcid": "N",
  "hydrogen": "Y",
  "helium": "N",
  "methane": "N",
  "waterVapor": "Y",
  "silicates": "Y",
  "iron": "Y",
  "nickel": "N"
}
```

Example PUT request:

```json
{
  "objectName": "Sample Exoplanet Updated",
  "category": "Exoplanet",
  "distanceLightYears": 42.123456,
  "discoveryDate": "2026-02-01T00:00:00Z",
  "inSolarSystem": "N",
  "habitabilityScore": 8.10,
  "surfaceTemperature": -10.0,
  "gravity": 1.05,
  "nitrogen": "Y",
  "oxygen": "Y",
  "co2": "N",
  "sulfuricAcid": "N",
  "hydrogen": "Y",
  "helium": "N",
  "methane": "N",
  "waterVapor": "Y",
  "silicates": "Y",
  "iron": "Y",
  "nickel": "N"
}
```

DELETE behavior:

- DELETE /api/celestial-objects/{id} returns 204 No Content on success.
- Returns 404 Not Found when the record does not exist.

Expected status codes:

- POST: 201 Created, 400 Bad Request, 409 Conflict
- PUT: 200 OK, 400 Bad Request, 404 Not Found
- DELETE: 204 No Content, 404 Not Found, 409 Conflict (related data constraints)

Validation rules for create/update requests:

- objectName is required, max 30 characters.
- category is required, max 50 characters, and must be one of:
  Planet, Exoplanet, Moon, Dwarf Planet, Asteroid, Comet, Black Hole, Neutron Star, Star.
- inSolarSystem must be Y or N.
- habitabilityScore must be between 0 and 10.
- gravity must be between 0 and 100 when provided.
- composition flags must each be Y or N:
  nitrogen, oxygen, co2, sulfuricAcid, hydrogen, helium, methane, waterVapor, silicates, iron, nickel.
- objectId (POST) must be greater than 0.

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

Release verification commands:

```powershell
dotnet restore
dotnet build --configuration Release --no-restore
dotnet test --configuration Release --no-build
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

## Runtime configuration

The backend reads configuration from appsettings, environment variables, and User Secrets.

Required for production hosting:

- `ConnectionStrings__OracleDb`
- `AllowedOrigins__0` (and additional indexes such as `AllowedOrigins__1` when needed)
- `ASPNETCORE_ENVIRONMENT` (typically `Production`)
- `ASPNETCORE_URLS` (for containers, typically `http://+:5000`)

### CORS behavior

- Development preserves local frontend origins (`http://localhost:4200`, `https://localhost:4200`).
- Production uses configured `AllowedOrigins` values only.
- Duplicate origins are removed automatically.
- Wildcard CORS is intentionally not enabled.

Example production origin config:

```powershell
set AllowedOrigins__0=https://your-frontend-app.azurestaticapps.net
set AllowedOrigins__1=https://your-frontend-app.azurewebsites.net
```

### Oracle connection configuration

- Do not store Oracle passwords in appsettings files.
- Local development should continue using User Secrets.
- Docker and cloud hosting should provide `ConnectionStrings__OracleDb` via environment variables.

Example local User Secrets:

```powershell
dotnet user-secrets set "ConnectionStrings:OracleDb" "User Id=YOUR_USER;Password=YOUR_PASSWORD;Data Source=127.0.0.1:1522/FREEPDB1;"
```

### Reverse proxy and HTTPS behavior

- Forwarded headers (`X-Forwarded-For`, `X-Forwarded-Proto`) are enabled for reverse-proxy hosting (Azure App Service / Azure Container Apps).
- HTTPS redirection remains enabled and works with forwarded headers to avoid redirect loops behind TLS-terminating proxies.
- Production exception responses return generic ProblemDetails payloads while full exception details remain logged server-side.

## Docker

This repository now includes a production Dockerfile at the backend root.

Build production image:

```powershell
docker build -t astro-track-backend:prod .
```

Run production image locally (example values):

```powershell
docker run --rm -p 5001:5000 ^
  -e ASPNETCORE_ENVIRONMENT=Production ^
  -e ASPNETCORE_URLS=http://+:5000 ^
  -e ConnectionStrings__OracleDb="User Id=YOUR_USER;Password=YOUR_PASSWORD;Data Source=host.docker.internal:1522/FREEPDB1;" ^
  -e AllowedOrigins__0=http://localhost:4200 ^
  astro-track-backend:prod
```

The existing local Docker Compose workflow remains supported via environment-variable connection string injection.

## Frontend production compatibility

The frontend production container calls same-origin `/api` and its Nginx proxy forwards requests to the backend origin.
For production deployment, set backend CORS origins to the frontend hostnames that will call the API.

## Azure deployment notes

- Azure App Service / Azure Container Apps should provide all production values via application settings / environment variables.
- Set `ConnectionStrings__OracleDb` in secret configuration.
- Set at least one `AllowedOrigins__*` value for the deployed frontend origin.
- Set `ASPNETCORE_ENVIRONMENT=Production` and `ASPNETCORE_URLS=http://+:5000` for container hosting.

## Health endpoint verification

With the API running, verify:

```powershell
curl http://localhost:5000/health
curl http://localhost:5000/health/database
```

- `/health` validates service liveness without requiring database connectivity.
- `/health/database` validates Oracle connectivity and CELESTIALOBJECTS queryability.

## Notes

- This repository is Database First
- EF Core migrations are intentionally not used
- Backend changes do not create or modify Oracle schema objects