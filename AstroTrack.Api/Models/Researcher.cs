namespace AstroTrack.Api.Models;

/// <summary>
/// Represents the RESEARCHERS table from the Oracle schema.
/// </summary>
public class Researcher
{
    public long ResearcherId { get; set; }
    public string ResearcherName { get; set; } = string.Empty;
    public string? ContactEmail { get; set; }
    public string? PhoneNumber { get; set; }
    public long AffiliationId { get; set; }
}