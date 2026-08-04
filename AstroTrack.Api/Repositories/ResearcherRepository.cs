using AstroTrack.Api.Data;
using AstroTrack.Api.Models;
using Microsoft.EntityFrameworkCore;

namespace AstroTrack.Api.Repositories;

public class ResearcherRepository : IResearcherRepository
{
    private readonly AstroTrackDbContext _dbContext;

    public ResearcherRepository(AstroTrackDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<IEnumerable<Researcher>> GetAllAsync()
    {
        return await _dbContext.Researchers
            .AsNoTracking()
            .OrderBy(researcher => researcher.ResearcherId)
            .ToListAsync();
    }

    public async Task<Researcher?> GetByIdAsync(long id)
    {
        return await _dbContext.Researchers
            .AsNoTracking()
            .FirstOrDefaultAsync(researcher => researcher.ResearcherId == id);
    }

    public async Task<bool> ExistsAsync(long id)
    {
        return await _dbContext.Researchers
            .AnyAsync(researcher => researcher.ResearcherId == id);
    }

    public async Task AddAsync(Researcher entity)
    {
        await _dbContext.Researchers.AddAsync(entity);
    }

    public Task UpdateAsync(Researcher entity)
    {
        _dbContext.Researchers.Update(entity);
        return Task.CompletedTask;
    }

    public Task DeleteAsync(Researcher entity)
    {
        _dbContext.Researchers.Remove(entity);
        return Task.CompletedTask;
    }

    public async Task<int> SaveChangesAsync()
    {
        return await _dbContext.SaveChangesAsync();
    }
}