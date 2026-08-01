using AstroTrack.Api.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Diagnostics.HealthChecks;

namespace AstroTrack.Api.Infrastructure;

/// <summary>
/// Verifies that the configured Oracle connection is reachable and that the CELESTIALOBJECTS table is queryable.
/// </summary>
public sealed class OracleDatabaseHealthCheck : IHealthCheck
{
    private readonly AstroTrackDbContext _dbContext;
    private readonly ILogger<OracleDatabaseHealthCheck> _logger;

    public OracleDatabaseHealthCheck(AstroTrackDbContext dbContext, ILogger<OracleDatabaseHealthCheck> logger)
    {
        _dbContext = dbContext;
        _logger = logger;
    }

    public async Task<HealthCheckResult> CheckHealthAsync(HealthCheckContext context, CancellationToken cancellationToken = default)
    {
        try
        {
            await _dbContext.Database.OpenConnectionAsync(cancellationToken);
            await _dbContext.CelestialObjects.AnyAsync(cancellationToken);
            await _dbContext.Database.CloseConnectionAsync();

            return HealthCheckResult.Healthy("Oracle connection is available and the CELESTIALOBJECTS table is queryable.");
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Oracle database health check failed.");
            return HealthCheckResult.Unhealthy("Oracle database connectivity check failed. Verify that the Oracle instance is running and the configured connection string is correct.");
        }
    }
}
