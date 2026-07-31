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

### Important notes

- This project is Database First.
- EF Core migrations must not be created or run for this repository.
- No database objects are created or modified by this issue.
