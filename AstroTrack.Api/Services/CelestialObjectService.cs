using AstroTrack.Api.DTOs.CelestialObjects;
using AstroTrack.Api.Models;
using AstroTrack.Api.Repositories;

namespace AstroTrack.Api.Services;

public class CelestialObjectService : ICelestialObjectService
{
    private readonly ICelestialObjectRepository _celestialObjectRepository;

    public CelestialObjectService(ICelestialObjectRepository celestialObjectRepository)
    {
        _celestialObjectRepository = celestialObjectRepository;
    }

    public async Task<IEnumerable<CelestialObjectDto>> GetAllAsync()
    {
        var celestialObjects = await _celestialObjectRepository.GetAllAsync();
        return celestialObjects.Select(MapToDto).ToList();
    }

    public async Task<CelestialObjectDto?> GetByIdAsync(long id)
    {
        var celestialObject = await _celestialObjectRepository.GetByIdAsync(id);
        return celestialObject is null ? null : MapToDto(celestialObject);
    }

    private static CelestialObjectDto MapToDto(CelestialObject celestialObject)
    {
        return new CelestialObjectDto
        {
            ObjectId = celestialObject.ObjectId,
            ObjectName = celestialObject.ObjectName,
            Category = celestialObject.Category,
            DistanceLightYears = celestialObject.DistanceLightYears,
            DiscoveryDate = celestialObject.DiscoveryDate,
            InSolarSystem = celestialObject.InSolarSystem,
            HabitabilityScore = celestialObject.HabitabilityScore,
            SurfaceTemperature = celestialObject.SurfaceTemperature,
            Gravity = celestialObject.Gravity,
            Nitrogen = celestialObject.Nitrogen,
            Oxygen = celestialObject.Oxygen,
            Co2 = celestialObject.Co2,
            SulfuricAcid = celestialObject.SulfuricAcid,
            Hydrogen = celestialObject.Hydrogen,
            Helium = celestialObject.Helium,
            Methane = celestialObject.Methane,
            WaterVapor = celestialObject.WaterVapor,
            Silicates = celestialObject.Silicates,
            Iron = celestialObject.Iron,
            Nickel = celestialObject.Nickel
        };
    }
}
