using AstroTrack.Api.DTOs.CelestialObjects;
using AstroTrack.Api.Models;
using AstroTrack.Api.Repositories;
using Microsoft.EntityFrameworkCore;

namespace AstroTrack.Api.Services;

public class CelestialObjectService : ICelestialObjectService
{
    private readonly ICelestialObjectRepository _celestialObjectRepository;
    private readonly ILogger<CelestialObjectService> _logger;

    public CelestialObjectService(
        ICelestialObjectRepository celestialObjectRepository,
        ILogger<CelestialObjectService> logger)
    {
        _celestialObjectRepository = celestialObjectRepository;
        _logger = logger;
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

    public async Task<CelestialObjectMutationResult> CreateAsync(CreateCelestialObjectDto dto)
    {
        if (await _celestialObjectRepository.ExistsAsync(dto.ObjectId))
        {
            return CelestialObjectMutationResult.Duplicate($"A celestial object with ID {dto.ObjectId} already exists.");
        }

        var entity = MapCreateDtoToEntity(dto);

        try
        {
            await _celestialObjectRepository.AddAsync(entity);
            await _celestialObjectRepository.SaveChangesAsync();
            return CelestialObjectMutationResult.Success(MapToDto(entity));
        }
        catch (DbUpdateException exception) when (IsDuplicateKeyViolation(exception))
        {
            _logger.LogWarning(exception, "Duplicate key violation while creating celestial object with ID {ObjectId}", dto.ObjectId);
            return CelestialObjectMutationResult.Duplicate($"A celestial object with ID {dto.ObjectId} already exists.");
        }
        catch (DbUpdateException exception) when (IsConstraintViolation(exception))
        {
            _logger.LogWarning(exception, "Constraint violation while creating celestial object with ID {ObjectId}", dto.ObjectId);
            return CelestialObjectMutationResult.ValidationFailed("The request violates one or more database constraints.");
        }
    }

    public async Task<CelestialObjectMutationResult> UpdateAsync(long id, UpdateCelestialObjectDto dto)
    {
        var existingEntity = await _celestialObjectRepository.GetByIdAsync(id);
        if (existingEntity is null)
        {
            return CelestialObjectMutationResult.NotFound($"Celestial object with ID {id} was not found.");
        }

        ApplyUpdateDto(existingEntity, dto);

        try
        {
            await _celestialObjectRepository.UpdateAsync(existingEntity);
            await _celestialObjectRepository.SaveChangesAsync();
            return CelestialObjectMutationResult.Success(MapToDto(existingEntity));
        }
        catch (DbUpdateException exception) when (IsConstraintViolation(exception))
        {
            _logger.LogWarning(exception, "Constraint violation while updating celestial object with ID {ObjectId}", id);
            return CelestialObjectMutationResult.ValidationFailed("The request violates one or more database constraints.");
        }
    }

    public async Task<CelestialObjectMutationResult> DeleteAsync(long id)
    {
        var existingEntity = await _celestialObjectRepository.GetByIdAsync(id);
        if (existingEntity is null)
        {
            return CelestialObjectMutationResult.NotFound($"Celestial object with ID {id} was not found.");
        }

        try
        {
            await _celestialObjectRepository.DeleteAsync(existingEntity);
            await _celestialObjectRepository.SaveChangesAsync();
            return CelestialObjectMutationResult.Success();
        }
        catch (DbUpdateException exception) when (IsConstraintViolation(exception))
        {
            _logger.LogWarning(exception, "Constraint violation while deleting celestial object with ID {ObjectId}", id);
            return CelestialObjectMutationResult.ValidationFailed("The record cannot be deleted because of related data constraints.");
        }
    }

    private static CelestialObject MapCreateDtoToEntity(CreateCelestialObjectDto dto)
    {
        return new CelestialObject
        {
            ObjectId = dto.ObjectId,
            ObjectName = dto.ObjectName,
            Category = dto.Category,
            DistanceLightYears = dto.DistanceLightYears,
            DiscoveryDate = dto.DiscoveryDate,
            InSolarSystem = ParseYesNo(dto.InSolarSystem),
            HabitabilityScore = dto.HabitabilityScore,
            SurfaceTemperature = dto.SurfaceTemperature,
            Gravity = dto.Gravity,
            Nitrogen = ParseYesNo(dto.Nitrogen),
            Oxygen = ParseYesNo(dto.Oxygen),
            Co2 = ParseYesNo(dto.Co2),
            SulfuricAcid = ParseYesNo(dto.SulfuricAcid),
            Hydrogen = ParseYesNo(dto.Hydrogen),
            Helium = ParseYesNo(dto.Helium),
            Methane = ParseYesNo(dto.Methane),
            WaterVapor = ParseYesNo(dto.WaterVapor),
            Silicates = ParseYesNo(dto.Silicates),
            Iron = ParseYesNo(dto.Iron),
            Nickel = ParseYesNo(dto.Nickel)
        };
    }

    private static void ApplyUpdateDto(CelestialObject entity, UpdateCelestialObjectDto dto)
    {
        entity.ObjectName = dto.ObjectName;
        entity.Category = dto.Category;
        entity.DistanceLightYears = dto.DistanceLightYears;
        entity.DiscoveryDate = dto.DiscoveryDate;
        entity.InSolarSystem = ParseYesNo(dto.InSolarSystem);
        entity.HabitabilityScore = dto.HabitabilityScore;
        entity.SurfaceTemperature = dto.SurfaceTemperature;
        entity.Gravity = dto.Gravity;
        entity.Nitrogen = ParseYesNo(dto.Nitrogen);
        entity.Oxygen = ParseYesNo(dto.Oxygen);
        entity.Co2 = ParseYesNo(dto.Co2);
        entity.SulfuricAcid = ParseYesNo(dto.SulfuricAcid);
        entity.Hydrogen = ParseYesNo(dto.Hydrogen);
        entity.Helium = ParseYesNo(dto.Helium);
        entity.Methane = ParseYesNo(dto.Methane);
        entity.WaterVapor = ParseYesNo(dto.WaterVapor);
        entity.Silicates = ParseYesNo(dto.Silicates);
        entity.Iron = ParseYesNo(dto.Iron);
        entity.Nickel = ParseYesNo(dto.Nickel);
    }

    private static bool ParseYesNo(string value)
    {
        return string.Equals(value, "Y", StringComparison.OrdinalIgnoreCase);
    }

    private static bool IsDuplicateKeyViolation(DbUpdateException exception)
    {
        return GetFlattenedMessage(exception).Contains("ORA-00001", StringComparison.OrdinalIgnoreCase);
    }

    private static bool IsConstraintViolation(DbUpdateException exception)
    {
        var flattenedMessage = GetFlattenedMessage(exception);
        return flattenedMessage.Contains("ORA-02290", StringComparison.OrdinalIgnoreCase)
            || flattenedMessage.Contains("ORA-12899", StringComparison.OrdinalIgnoreCase)
            || flattenedMessage.Contains("ORA-01400", StringComparison.OrdinalIgnoreCase)
            || flattenedMessage.Contains("ORA-02291", StringComparison.OrdinalIgnoreCase)
            || flattenedMessage.Contains("ORA-02292", StringComparison.OrdinalIgnoreCase);
    }

    private static string GetFlattenedMessage(Exception exception)
    {
        var messages = new List<string>();
        Exception? current = exception;

        while (current is not null)
        {
            messages.Add(current.Message);
            current = current.InnerException;
        }

        return string.Join(" | ", messages);
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
