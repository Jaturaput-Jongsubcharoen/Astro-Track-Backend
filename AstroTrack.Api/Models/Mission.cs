namespace AstroTrack.Api.Models;

/// <summary>
/// Represents the MISSIONS table from the Oracle schema.
/// </summary>
public class Mission
{
    public long MissionId { get; set; }
    public string MissionName { get; set; } = string.Empty;
    public string MissionPurpose { get; set; } = string.Empty;
    public DateTime StartDate { get; set; }
    public DateTime? EndDate { get; set; }
    public long LeadResearcherId { get; set; }
    public long AffiliationId { get; set; }
}
