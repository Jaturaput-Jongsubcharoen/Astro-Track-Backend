namespace AstroTrack.Api.Models;

/// <summary>
/// Represents the CELESTIALOBJECTS table from the Oracle schema.
/// </summary>
public class CelestialObject
{
    public long ObjectId { get; set; }
    public string ObjectName { get; set; } = string.Empty;
    public string Category { get; set; } = string.Empty;
    public decimal? DistanceLightYears { get; set; }
    public DateTime? DiscoveryDate { get; set; }
    public bool InSolarSystem { get; set; }
    public decimal? HabitabilityScore { get; set; }
    public decimal? SurfaceTemperature { get; set; }
    public decimal? Gravity { get; set; }
    public bool Nitrogen { get; set; }
    public bool Oxygen { get; set; }
    public bool Co2 { get; set; }
    public bool SulfuricAcid { get; set; }
    public bool Hydrogen { get; set; }
    public bool Helium { get; set; }
    public bool Methane { get; set; }
    public bool WaterVapor { get; set; }
    public bool Silicates { get; set; }
    public bool Iron { get; set; }
    public bool Nickel { get; set; }
}
