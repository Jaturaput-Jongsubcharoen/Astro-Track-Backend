using AstroTrack.Api.DTOs.Missions;

namespace AstroTrack.Api.Services;

public interface IMissionService
{
    Task<IEnumerable<MissionDto>> GetAllAsync();

    Task<MissionDto?> GetByIdAsync(long id);

    Task<MissionMutationResult> CreateAsync(CreateMissionDto dto);

    Task<MissionMutationResult> UpdateAsync(long id, UpdateMissionDto dto);

    Task<MissionMutationResult> DeleteAsync(long id);
}
