using AstroTrack.Api.Models;

namespace AstroTrack.Api.Repositories;

public interface IMissionRepository
{
    Task<IEnumerable<Mission>> GetAllAsync();

    Task<Mission?> GetByIdAsync(long id);

    Task<bool> ExistsAsync(long id);

    Task AddAsync(Mission entity);

    Task UpdateAsync(Mission entity);

    Task DeleteAsync(Mission entity);

    Task<int> SaveChangesAsync();
}
