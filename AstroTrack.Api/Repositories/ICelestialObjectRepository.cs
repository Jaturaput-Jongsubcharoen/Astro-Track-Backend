using AstroTrack.Api.Models;

namespace AstroTrack.Api.Repositories;

public interface ICelestialObjectRepository
{
    Task<IEnumerable<CelestialObject>> GetAllAsync();

    Task<CelestialObject?> GetByIdAsync(long id);
}
