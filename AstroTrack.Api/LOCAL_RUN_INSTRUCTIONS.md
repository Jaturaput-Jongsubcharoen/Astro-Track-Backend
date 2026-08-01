# Local run instructions

1. Restore dependencies:
   `dotnet restore`
2. Build the project:
   `dotnet build`
3. Run the API:
   `dotnet run`
4. Configure the Oracle connection string locally (do not commit real credentials):
   - `.NET user secrets`: `dotnet user-secrets set "ConnectionStrings:OracleDb" "User Id=YOUR_USER;Password=YOUR_PASSWORD;Data Source=//localhost:1521/XEPDB1;"`
   - Environment variable: `setx ConnectionStrings__OracleDb "User Id=YOUR_USER;Password=YOUR_PASSWORD;Data Source=//localhost:1521/XEPDB1;"`
5. Ensure the Oracle instance is running and the Astro Track SQL scripts have been executed against it.
6. Browse:
   - Basic health endpoint: `http://localhost:5000/health`
   - Database health endpoint: `http://localhost:5000/health/database`
   - Swagger UI: `http://localhost:5000/swagger`
