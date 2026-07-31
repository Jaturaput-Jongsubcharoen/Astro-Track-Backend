using System.ComponentModel.DataAnnotations;

namespace AstroTrack.Api.DTOs.CelestialObjects;

/// <summary>
/// Create DTO for a celestial object.
/// </summary>
public class CreateCelestialObjectDto
{
    [Required]
    [StringLength(30)]
    public string ObjectName { get; set; } = string.Empty;

    [Required]
    [StringLength(50)]
    [RegularExpression(
        "^(Planet|Exoplanet|Moon|Dwarf Planet|Asteroid|Comet|Black Hole|Neutron Star|Star)$",
        ErrorMessage = "Category must be one of the allowed values from the Oracle schema.")]
    public string Category { get; set; } = string.Empty;

    [Range(0, double.MaxValue, ErrorMessage = "DistanceLightYears must be greater than or equal to 0")]
    public decimal? DistanceLightYears { get; set; }

    [DataType(DataType.Date)]
    public DateTime? DiscoveryDate { get; set; }

    public bool InSolarSystem { get; set; }

    [Range(0, 10, ErrorMessage = "HabitabilityScore must be between 0 and 10")]
    public decimal? HabitabilityScore { get; set; }

    public decimal? SurfaceTemperature { get; set; }

    [Range(0, 100, ErrorMessage = "Gravity must be between 0 and 100")]
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
