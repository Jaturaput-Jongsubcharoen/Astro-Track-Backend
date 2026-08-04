using AstroTrack.Api.Models;

namespace AstroTrack.Api.Repositories;

public interface IObservationRepository
{
    Task<IEnumerable<Observation>> GetAllAsync();

    Task<Observation?> GetByIdAsync(long id);

    Task<bool> ExistsAsync(long id);

    Task AddAsync(Observation entity);

    Task UpdateAsync(Observation entity);

    Task DeleteAsync(Observation entity);

    Task<int> SaveChangesAsync();
}
