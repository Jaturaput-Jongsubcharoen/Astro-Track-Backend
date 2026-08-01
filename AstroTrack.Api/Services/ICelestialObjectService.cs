using AstroTrack.Api.DTOs.CelestialObjects;

namespace AstroTrack.Api.Services;

public interface ICelestialObjectService
{
    Task<IEnumerable<CelestialObjectDto>> GetAllAsync();

    Task<CelestialObjectDto?> GetByIdAsync(long id);
}
