using AstroTrack.Api.Data;
using AstroTrack.Api.Models;
using Microsoft.EntityFrameworkCore;

namespace AstroTrack.Api.Repositories;

public class ObservationRepository : IObservationRepository
{
    private readonly AstroTrackDbContext _dbContext;

    public ObservationRepository(AstroTrackDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<IEnumerable<Observation>> GetAllAsync()
    {
        return await _dbContext.Observations
            .AsNoTracking()
            .OrderBy(observation => observation.ObservationId)
            .ToListAsync();
    }

    public async Task<Observation?> GetByIdAsync(long id)
    {
        return await _dbContext.Observations
            .AsNoTracking()
            .FirstOrDefaultAsync(observation => observation.ObservationId == id);
    }

    public async Task<bool> ExistsAsync(long id)
    {
        return await _dbContext.Observations
            .AnyAsync(observation => observation.ObservationId == id);
    }

    public async Task AddAsync(Observation entity)
    {
        await _dbContext.Observations.AddAsync(entity);
    }

    public Task UpdateAsync(Observation entity)
    {
        _dbContext.Observations.Update(entity);
        return Task.CompletedTask;
    }

    public Task DeleteAsync(Observation entity)
    {
        _dbContext.Observations.Remove(entity);
        return Task.CompletedTask;
    }

    public async Task<int> SaveChangesAsync()
    {
        return await _dbContext.SaveChangesAsync();
    }
}
