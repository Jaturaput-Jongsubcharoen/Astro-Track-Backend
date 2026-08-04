namespace AstroTrack.Api.Models;

/// <summary>
/// Represents the OBSERVATIONS table from the Oracle schema.
/// </summary>
public class Observation
{
    public long ObservationId { get; set; }
    public long ObjectId { get; set; }
    public long TelescopeId { get; set; }
    public long ResearcherId { get; set; }
    public DateTime ObservationDate { get; set; }
    public decimal? XrayFlux { get; set; }
    public decimal? Redshift { get; set; }
}
