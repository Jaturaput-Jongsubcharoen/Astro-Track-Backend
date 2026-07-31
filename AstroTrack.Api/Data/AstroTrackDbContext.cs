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
}
