using System.ComponentModel.DataAnnotations;

namespace AstroTrack.Api.DTOs.Researchers;

/// <summary>
/// Create DTO for a researcher.
/// </summary>
public class CreateResearcherDto
{
    [Range(1, long.MaxValue, ErrorMessage = "ResearcherId must be greater than 0")]
    public long ResearcherId { get; set; }

    [Required]
    [StringLength(30)]
    public string ResearcherName { get; set; } = string.Empty;

    [StringLength(50)]
    [RegularExpression(
        "^[A-Za-z0-9._%+-]+@[A-Za-z0-9.-]+\\.[A-Za-z]{2,}$",
        ErrorMessage = "ContactEmail must be a valid email address.")]
    public string? ContactEmail { get; set; }

    [StringLength(15)]
    [RegularExpression(
        "^\\+\\d{1,3}-\\d{1,4}-\\d{4,10}$",
        ErrorMessage = "PhoneNumber must match +<country>-<area>-<number> format.")]
    public string? PhoneNumber { get; set; }

    [Range(1, long.MaxValue, ErrorMessage = "AffiliationId must be greater than 0")]
    public long AffiliationId { get; set; }
}