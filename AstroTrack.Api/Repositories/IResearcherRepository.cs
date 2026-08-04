using AstroTrack.Api.Models;

namespace AstroTrack.Api.Repositories;

public interface IResearcherRepository
{
    Task<IEnumerable<Researcher>> GetAllAsync();

    Task<Researcher?> GetByIdAsync(long id);

    Task<bool> ExistsAsync(long id);

    Task AddAsync(Researcher entity);

    Task UpdateAsync(Researcher entity);

    Task DeleteAsync(Researcher entity);

    Task<int> SaveChangesAsync();
}