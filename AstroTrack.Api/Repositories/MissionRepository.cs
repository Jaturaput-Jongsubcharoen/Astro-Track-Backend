using AstroTrack.Api.Data;
using AstroTrack.Api.Models;
using Microsoft.EntityFrameworkCore;

namespace AstroTrack.Api.Repositories;

public class MissionRepository : IMissionRepository
{
    private readonly AstroTrackDbContext _dbContext;

    public MissionRepository(AstroTrackDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<IEnumerable<Mission>> GetAllAsync()
    {
        return await _dbContext.Missions
            .AsNoTracking()
            .OrderBy(mission => mission.MissionId)
            .ToListAsync();
    }

    public async Task<Mission?> GetByIdAsync(long id)
    {
        return await _dbContext.Missions
            .AsNoTracking()
            .FirstOrDefaultAsync(mission => mission.MissionId == id);
    }

    public async Task<bool> ExistsAsync(long id)
    {
        return await _dbContext.Missions
            .AnyAsync(mission => mission.MissionId == id);
    }

    public async Task AddAsync(Mission entity)
    {
        await _dbContext.Missions.AddAsync(entity);
    }

    public Task UpdateAsync(Mission entity)
    {
        _dbContext.Missions.Update(entity);
        return Task.CompletedTask;
    }

    public Task DeleteAsync(Mission entity)
    {
        _dbContext.Missions.Remove(entity);
        return Task.CompletedTask;
    }

    public async Task<int> SaveChangesAsync()
    {
        return await _dbContext.SaveChangesAsync();
    }
}
