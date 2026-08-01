# Local run instructions

1. Start and verify the Oracle Docker container:
   `docker ps -a`
   `docker start astrotrack-oracle`
   `docker ps`
2. Set the local environment to Development so .NET User Secrets are loaded:
   `set ASPNETCORE_ENVIRONMENT=Development`
   `echo %ASPNETCORE_ENVIRONMENT%`
3. Configure the Oracle connection string with .NET User Secrets (do not commit credentials):
   `dotnet user-secrets init`
   `dotnet user-secrets set "ConnectionStrings:OracleDb" "User Id=YOUR_USER;Password=YOUR_PASSWORD;Data Source=127.0.0.1:1522/FREEPDB1;"`
4. Restore dependencies:
   `dotnet restore`
5. Build the project:
   `dotnet build`
6. Run the API:
   `dotnet run`
7. Browse:
   - Basic health endpoint: `http://localhost:5000/health`
   - Database health endpoint: `http://localhost:5000/health/database`
   - Read-only celestial objects endpoint: `http://localhost:5000/api/celestial-objects`
   - Read-only celestial object by ID endpoint: `http://localhost:5000/api/celestial-objects/1`
   - Swagger UI: `http://localhost:5000/swagger`
