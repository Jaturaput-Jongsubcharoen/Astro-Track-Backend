using AstroTrack.Api.Models;

namespace AstroTrack.Api.Repositories;

public interface ICelestialObjectRepository
{
    Task<IEnumerable<CelestialObject>> GetAllAsync();

    Task<CelestialObject?> GetByIdAsync(long id);

    Task<bool> ExistsAsync(long id);

    Task AddAsync(CelestialObject entity);

    Task UpdateAsync(CelestialObject entity);

    Task DeleteAsync(CelestialObject entity);

    Task<int> SaveChangesAsync();
}
