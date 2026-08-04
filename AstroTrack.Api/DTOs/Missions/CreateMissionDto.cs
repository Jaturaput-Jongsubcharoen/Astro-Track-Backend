using System.ComponentModel.DataAnnotations;

namespace AstroTrack.Api.DTOs.Missions;

/// <summary>
/// Create DTO for a mission.
/// </summary>
public class CreateMissionDto
{
    [Range(1, long.MaxValue, ErrorMessage = "MissionId must be greater than 0")]
    public long MissionId { get; set; }

    [Required]
    [StringLength(30)]
    public string MissionName { get; set; } = string.Empty;

    [Required]
    [StringLength(100)]
    [RegularExpression("^.*\\S.*$", ErrorMessage = "MissionPurpose cannot be empty or whitespace.")]
    public string MissionPurpose { get; set; } = string.Empty;

    [DataType(DataType.Date)]
    public DateTime StartDate { get; set; }

    [DataType(DataType.Date)]
    public DateTime? EndDate { get; set; }

    [Range(1, long.MaxValue, ErrorMessage = "LeadResearcherId must be greater than 0")]
    public long LeadResearcherId { get; set; }

    [Range(1, long.MaxValue, ErrorMessage = "AffiliationId must be greater than 0")]
    public long AffiliationId { get; set; }
}
