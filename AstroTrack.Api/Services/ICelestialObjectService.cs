using AstroTrack.Api.DTOs.CelestialObjects;

namespace AstroTrack.Api.Services;

public interface ICelestialObjectService
{
    Task<IEnumerable<CelestialObjectDto>> GetAllAsync();

    Task<CelestialObjectDto?> GetByIdAsync(long id);

    Task<CelestialObjectMutationResult> CreateAsync(CreateCelestialObjectDto dto);

    Task<CelestialObjectMutationResult> UpdateAsync(long id, UpdateCelestialObjectDto dto);

    Task<CelestialObjectMutationResult> DeleteAsync(long id);
}
