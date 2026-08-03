using System.ComponentModel.DataAnnotations;

namespace AstroTrack.Api.DTOs.CelestialObjects;

/// <summary>
/// Create DTO for a celestial object.
/// </summary>
public class CreateCelestialObjectDto
{
    [Range(1, long.MaxValue, ErrorMessage = "ObjectId must be greater than 0")]
    public long ObjectId { get; set; }

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

    [Required]
    [RegularExpression("^[YN]$", ErrorMessage = "InSolarSystem must be 'Y' or 'N'.")]
    public string InSolarSystem { get; set; } = "N";

    [Range(0, 10, ErrorMessage = "HabitabilityScore must be between 0 and 10")]
    public decimal? HabitabilityScore { get; set; }

    public decimal? SurfaceTemperature { get; set; }

    [Range(0, 100, ErrorMessage = "Gravity must be between 0 and 100")]
    public decimal? Gravity { get; set; }

    [Required]
    [RegularExpression("^[YN]$", ErrorMessage = "Nitrogen must be 'Y' or 'N'.")]
    public string Nitrogen { get; set; } = "N";

    [Required]
    [RegularExpression("^[YN]$", ErrorMessage = "Oxygen must be 'Y' or 'N'.")]
    public string Oxygen { get; set; } = "N";

    [Required]
    [RegularExpression("^[YN]$", ErrorMessage = "Co2 must be 'Y' or 'N'.")]
    public string Co2 { get; set; } = "N";

    [Required]
    [RegularExpression("^[YN]$", ErrorMessage = "SulfuricAcid must be 'Y' or 'N'.")]
    public string SulfuricAcid { get; set; } = "N";

    [Required]
    [RegularExpression("^[YN]$", ErrorMessage = "Hydrogen must be 'Y' or 'N'.")]
    public string Hydrogen { get; set; } = "N";

    [Required]
    [RegularExpression("^[YN]$", ErrorMessage = "Helium must be 'Y' or 'N'.")]
    public string Helium { get; set; } = "N";

    [Required]
    [RegularExpression("^[YN]$", ErrorMessage = "Methane must be 'Y' or 'N'.")]
    public string Methane { get; set; } = "N";

    [Required]
    [RegularExpression("^[YN]$", ErrorMessage = "WaterVapor must be 'Y' or 'N'.")]
    public string WaterVapor { get; set; } = "N";

    [Required]
    [RegularExpression("^[YN]$", ErrorMessage = "Silicates must be 'Y' or 'N'.")]
    public string Silicates { get; set; } = "N";

    [Required]
    [RegularExpression("^[YN]$", ErrorMessage = "Iron must be 'Y' or 'N'.")]
    public string Iron { get; set; } = "N";

    [Required]
    [RegularExpression("^[YN]$", ErrorMessage = "Nickel must be 'Y' or 'N'.")]
    public string Nickel { get; set; } = "N";
}
