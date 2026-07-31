using AstroTrack.Api.Data.Configurations;
using AstroTrack.Api.Models;
using Microsoft.EntityFrameworkCore;

namespace AstroTrack.Api.Data;

/// <summary>
/// Minimal DbContext for the AstroTrack backend.
/// This project uses a Database First approach and does not create migrations.
/// </summary>
public class AstroTrackDbContext : DbContext
{
    public AstroTrackDbContext(DbContextOptions<AstroTrackDbContext> options)
        : base(options)
    {
    }

    public DbSet<CelestialObject> CelestialObjects { get; set; } = null!;

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);
        modelBuilder.ApplyConfiguration(new CelestialObjectConfiguration());
    }
}
