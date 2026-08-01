# Astro-Track-Backend
ASP.NET Core backend and REST API for the Astro Track astronomy management platform.

## Oracle EF Core configuration

This backend uses a Database First approach with the Oracle database schema defined in the Astro-Track-Oracle-SQL project. The .NET project does not create or run EF Core migrations.

### Configure OracleDb locally

Use one of these safe local-development options:

- .NET user secrets:
  - `dotnet user-secrets set "ConnectionStrings:OracleDb" "User Id=YOUR_USER;Password=YOUR_PASSWORD;Data Source=//localhost:1521/XEPDB1;"`
- Environment variable:
  - `setx ConnectionStrings__OracleDb "User Id=YOUR_USER;Password=YOUR_PASSWORD;Data Source=//localhost:1521/XEPDB1;"`

The placeholder values above are intentionally not real credentials. The project reads the connection string from `ConnectionStrings:OracleDb` in the application configuration.

### Verify the Oracle instance is running

Before testing the health check, confirm that the Oracle database instance is available:

- Ensure the Oracle listener is running and the target service is reachable.
- Verify the hostname, port, and service name in the connection string.
- If using a local Oracle container or XE instance, confirm it is up before launching the API.

### Prepare the Astro Track schema

Before testing connectivity, execute the Astro Track Oracle SQL scripts from the sibling repository:

- `Astro-Track-Oracle-SQL/sql/Astro_Track_Project.sql`
- Any additional script required by your local Oracle environment for the schema objects used by this backend.

Do not modify the Oracle SQL repository as part of this issue.

### Test the database health endpoint

Once the API is running, call:

- `http://localhost:5000/health/database`

A successful response indicates that the configured Oracle connection was opened and the `CELESTIALOBJECTS` table could be queried.

### Important notes

- This project is Database First.
- EF Core migrations must not be created or run for this repository.
- No database objects are created or modified by this issue.
