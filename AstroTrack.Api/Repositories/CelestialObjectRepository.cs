using AstroTrack.Api.Data;
using AstroTrack.Api.Models;
using Microsoft.EntityFrameworkCore;

namespace AstroTrack.Api.Repositories;

public class CelestialObjectRepository : ICelestialObjectRepository
{
    private readonly AstroTrackDbContext _dbContext;

    public CelestialObjectRepository(AstroTrackDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<IEnumerable<CelestialObject>> GetAllAsync()
    {
        return await _dbContext.CelestialObjects
            .AsNoTracking()
            .OrderBy(celestialObject => celestialObject.ObjectId)
            .ToListAsync();
    }

    public async Task<CelestialObject?> GetByIdAsync(long id)
    {
        return await _dbContext.CelestialObjects
            .AsNoTracking()
            .FirstOrDefaultAsync(celestialObject => celestialObject.ObjectId == id);
    }

    public async Task<bool> ExistsAsync(long id)
    {
        return await _dbContext.CelestialObjects
            .AnyAsync(celestialObject => celestialObject.ObjectId == id);
    }

    public async Task AddAsync(CelestialObject entity)
    {
        await _dbContext.CelestialObjects.AddAsync(entity);
    }

    public Task UpdateAsync(CelestialObject entity)
    {
        _dbContext.CelestialObjects.Update(entity);
        return Task.CompletedTask;
    }

    public Task DeleteAsync(CelestialObject entity)
    {
        _dbContext.CelestialObjects.Remove(entity);
        return Task.CompletedTask;
    }

    public async Task<int> SaveChangesAsync()
    {
        return await _dbContext.SaveChangesAsync();
    }
}
