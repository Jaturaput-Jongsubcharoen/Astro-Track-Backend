using AstroTrack.Api.Data;
using AstroTrack.Api.Infrastructure;
using AstroTrack.Api.Repositories;
using AstroTrack.Api.Services;
using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.HttpOverrides;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Diagnostics.HealthChecks;

var builder = WebApplication.CreateBuilder(args);

var allowedOrigins = ResolveAllowedOrigins(builder.Configuration, builder.Environment);

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.Services.AddProblemDetails();

var connectionString = builder.Configuration.GetConnectionString("OracleDb");
if (string.IsNullOrWhiteSpace(connectionString))
{
    throw new InvalidOperationException("Connection string 'OracleDb' was not found. Configure it with appsettings.json, appsettings.Development.json, environment variables, or .NET user secrets.");
}

builder.Services.AddDbContext<AstroTrackDbContext>(options =>
    options.UseOracle(connectionString));

builder.Services.AddScoped<ICelestialObjectRepository, CelestialObjectRepository>();
builder.Services.AddScoped<ICelestialObjectService, CelestialObjectService>();
builder.Services.AddScoped<IResearcherRepository, ResearcherRepository>();
builder.Services.AddScoped<IResearcherService, ResearcherService>();
builder.Services.AddScoped<IMissionRepository, MissionRepository>();
builder.Services.AddScoped<IMissionService, MissionService>();
builder.Services.AddScoped<IObservationRepository, ObservationRepository>();
builder.Services.AddScoped<IObservationService, ObservationService>();

builder.Services.AddHealthChecks()
    .AddCheck<OracleDatabaseHealthCheck>("oracle-database");

builder.Services.AddCors(options =>
{
    options.AddPolicy("ConfiguredCorsOrigins", policy =>
    {
        policy.WithOrigins(allowedOrigins)
              .AllowAnyHeader()
              .AllowAnyMethod();
    });
});

builder.Services.Configure<ForwardedHeadersOptions>(options =>
{
    options.ForwardedHeaders = ForwardedHeaders.XForwardedFor | ForwardedHeaders.XForwardedProto;

    // Trust reverse proxy forwarding headers in containerized hosting environments.
    options.KnownNetworks.Clear();
    options.KnownProxies.Clear();
});

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}
else
{
    app.UseExceptionHandler(errorApp =>
    {
        errorApp.Run(async context =>
        {
            var exceptionFeature = context.Features.Get<IExceptionHandlerFeature>();
            var logger = context.RequestServices
                .GetRequiredService<ILoggerFactory>()
                .CreateLogger("GlobalExceptionHandler");

            if (exceptionFeature?.Error is not null)
            {
                logger.LogError(
                    exceptionFeature.Error,
                    "Unhandled exception while processing {Method} {Path}",
                    context.Request.Method,
                    context.Request.Path);
            }

            context.Response.StatusCode = StatusCodes.Status500InternalServerError;

            await Results.Problem(
                title: "An unexpected error occurred while processing the request.",
                statusCode: StatusCodes.Status500InternalServerError)
                .ExecuteAsync(context);
        });
    });
}

app.UseForwardedHeaders();
app.UseHttpsRedirection();
app.UseCors("ConfiguredCorsOrigins");
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

static string[] ResolveAllowedOrigins(IConfiguration configuration, IHostEnvironment environment)
{
    var configuredOrigins = configuration
        .GetSection("AllowedOrigins")
        .Get<string[]>() ?? Array.Empty<string>();

    var devDefaults = environment.IsDevelopment()
        ? new[] { "http://localhost:4200", "https://localhost:4200" }
        : Array.Empty<string>();

    var origins = configuredOrigins
        .Concat(devDefaults)
        .Where(origin => !string.IsNullOrWhiteSpace(origin))
        .Select(origin => origin.Trim().TrimEnd('/'))
        .Distinct(StringComparer.OrdinalIgnoreCase)
        .ToArray();

    if (origins.Length == 0)
    {
        throw new InvalidOperationException(
            "No CORS origins were configured. Set AllowedOrigins in configuration, for example with AllowedOrigins__0 environment variable.");
    }

    return origins;
}
