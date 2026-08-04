using System.ComponentModel.DataAnnotations;

namespace AstroTrack.Api.DTOs.Observations;

/// <summary>
/// Update DTO for an observation.
/// </summary>
public class UpdateObservationDto
{
    [Range(1, long.MaxValue, ErrorMessage = "ObjectId must be greater than 0")]
    public long ObjectId { get; set; }

    [Range(1, long.MaxValue, ErrorMessage = "TelescopeId must be greater than 0")]
    public long TelescopeId { get; set; }

    [Range(1, long.MaxValue, ErrorMessage = "ResearcherId must be greater than 0")]
    public long ResearcherId { get; set; }

    [DataType(DataType.Date)]
    public DateTime ObservationDate { get; set; }

    [Range(0, double.MaxValue, ErrorMessage = "XrayFlux must be greater than or equal to 0")]
    public decimal? XrayFlux { get; set; }

    [Range(-1, 10, ErrorMessage = "Redshift must be between -1 and 10")]
    public decimal? Redshift { get; set; }
}
