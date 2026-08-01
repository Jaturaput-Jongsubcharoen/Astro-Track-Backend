using AstroTrack.Api.Data;
using AstroTrack.Api.Infrastructure;
using AstroTrack.Api.Repositories;
using AstroTrack.Api.Services;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Diagnostics.HealthChecks;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var connectionString = builder.Configuration.GetConnectionString("OracleDb");
if (string.IsNullOrWhiteSpace(connectionString))
{
    throw new InvalidOperationException("Connection string 'OracleDb' was not found. Configure it with appsettings.json, appsettings.Development.json, environment variables, or .NET user secrets.");
}

builder.Services.AddDbContext<AstroTrackDbContext>(options =>
    options.UseOracle(connectionString));

builder.Services.AddScoped<ICelestialObjectRepository, CelestialObjectRepository>();
builder.Services.AddScoped<ICelestialObjectService, CelestialObjectService>();

builder.Services.AddHealthChecks()
    .AddCheck<OracleDatabaseHealthCheck>("oracle-database");

builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAngularDev", policy =>
    {
        policy.WithOrigins("http://localhost:4200", "https://localhost:4200")
              .AllowAnyHeader()
              .AllowAnyMethod();
    });
});

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();
app.UseCors("AllowAngularDev");
app.UseAuthorization();

app.MapControllers();

app.MapGet("/health", () => Results.Ok(new
{
    status = "healthy",
    timestamp = DateTime.UtcNow
}));

app.MapGet("/health/database", async (
    HealthCheckService healthCheckService,
    CancellationToken cancellationToken) =>
{
    var result = await healthCheckService.CheckHealthAsync(
        registration => registration.Name == "oracle-database",
        cancellationToken);

    return result.Status == HealthStatus.Healthy
        ? Results.Ok(new
        {
            status = "healthy",
            database = "oracle",
            check = "CELESTIALOBJECTS",
            timestamp = DateTime.UtcNow
        })
        : Results.Json(
            new
            {
                status = "unhealthy",
                database = "oracle",
                check = "CELESTIALOBJECTS",
                timestamp = DateTime.UtcNow
            },
            statusCode: StatusCodes.Status503ServiceUnavailable);
});

app.Run();
