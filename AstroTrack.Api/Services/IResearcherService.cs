using AstroTrack.Api.DTOs.Researchers;

namespace AstroTrack.Api.Services;

public interface IResearcherService
{
    Task<IEnumerable<ResearcherDto>> GetAllAsync();

    Task<ResearcherDto?> GetByIdAsync(long id);

    Task<ResearcherMutationResult> CreateAsync(CreateResearcherDto dto);

    Task<ResearcherMutationResult> UpdateAsync(long id, UpdateResearcherDto dto);

    Task<ResearcherMutationResult> DeleteAsync(long id);
}