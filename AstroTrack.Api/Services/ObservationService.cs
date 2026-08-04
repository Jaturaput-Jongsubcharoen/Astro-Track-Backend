using AstroTrack.Api.DTOs.Observations;
using AstroTrack.Api.Models;
using AstroTrack.Api.Repositories;
using Microsoft.EntityFrameworkCore;

namespace AstroTrack.Api.Services;

public class ObservationService : IObservationService
{
    private readonly IObservationRepository _observationRepository;
    private readonly ILogger<ObservationService> _logger;

    public ObservationService(
        IObservationRepository observationRepository,
        ILogger<ObservationService> logger)
    {
        _observationRepository = observationRepository;
        _logger = logger;
    }

    public async Task<IEnumerable<ObservationDto>> GetAllAsync()
    {
        var observations = await _observationRepository.GetAllAsync();
        return observations.Select(MapToDto).ToList();
    }

    public async Task<ObservationDto?> GetByIdAsync(long id)
    {
        var observation = await _observationRepository.GetByIdAsync(id);
        return observation is null ? null : MapToDto(observation);
    }

    public async Task<ObservationMutationResult> CreateAsync(CreateObservationDto dto)
    {
        if (await _observationRepository.ExistsAsync(dto.ObservationId))
        {
            return ObservationMutationResult.Duplicate($"An observation with ID {dto.ObservationId} already exists.");
        }

        var entity = MapCreateDtoToEntity(dto);

        try
        {
            await _observationRepository.AddAsync(entity);
            await _observationRepository.SaveChangesAsync();
            return ObservationMutationResult.Success(MapToDto(entity));
        }
        catch (DbUpdateException exception) when (IsDuplicateKeyViolation(exception))
        {
            _logger.LogWarning(exception, "Duplicate key violation while creating observation with ID {ObservationId}", dto.ObservationId);
            return ObservationMutationResult.Duplicate($"An observation with ID {dto.ObservationId} already exists.");
        }
        catch (DbUpdateException exception) when (IsConstraintViolation(exception))
        {
            _logger.LogWarning(exception, "Constraint violation while creating observation with ID {ObservationId}", dto.ObservationId);
            return ObservationMutationResult.ValidationFailed("The request violates one or more database constraints.");
        }
    }

    public async Task<ObservationMutationResult> UpdateAsync(long id, UpdateObservationDto dto)
    {
        var existingEntity = await _observationRepository.GetByIdAsync(id);
        if (existingEntity is null)
        {
            return ObservationMutationResult.NotFound($"Observation with ID {id} was not found.");
        }

        ApplyUpdateDto(existingEntity, dto);

        try
        {
            await _observationRepository.UpdateAsync(existingEntity);
            await _observationRepository.SaveChangesAsync();
            return ObservationMutationResult.Success(MapToDto(existingEntity));
        }
        catch (DbUpdateException exception) when (IsConstraintViolation(exception))
        {
            _logger.LogWarning(exception, "Constraint violation while updating observation with ID {ObservationId}", id);
            return ObservationMutationResult.ValidationFailed("The request violates one or more database constraints.");
        }
    }

    public async Task<ObservationMutationResult> DeleteAsync(long id)
    {
        var existingEntity = await _observationRepository.GetByIdAsync(id);
        if (existingEntity is null)
        {
            return ObservationMutationResult.NotFound($"Observation with ID {id} was not found.");
        }

        try
        {
            await _observationRepository.DeleteAsync(existingEntity);
            await _observationRepository.SaveChangesAsync();
            return ObservationMutationResult.Success();
        }
        catch (DbUpdateException exception) when (IsConstraintViolation(exception))
        {
            _logger.LogWarning(exception, "Constraint violation while deleting observation with ID {ObservationId}", id);
            return ObservationMutationResult.ValidationFailed("The record cannot be deleted because of related data constraints.");
        }
    }

    private static Observation MapCreateDtoToEntity(CreateObservationDto dto)
    {
        return new Observation
        {
            ObservationId = dto.ObservationId,
            ObjectId = dto.ObjectId,
            TelescopeId = dto.TelescopeId,
            ResearcherId = dto.ResearcherId,
            ObservationDate = dto.ObservationDate,
            XrayFlux = dto.XrayFlux,
            Redshift = dto.Redshift
        };
    }

    private static void ApplyUpdateDto(Observation entity, UpdateObservationDto dto)
    {
        entity.ObjectId = dto.ObjectId;
        entity.TelescopeId = dto.TelescopeId;
        entity.ResearcherId = dto.ResearcherId;
        entity.ObservationDate = dto.ObservationDate;
        entity.XrayFlux = dto.XrayFlux;
        entity.Redshift = dto.Redshift;
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

    private static ObservationDto MapToDto(Observation observation)
    {
        return new ObservationDto
        {
            ObservationId = observation.ObservationId,
            ObjectId = observation.ObjectId,
            TelescopeId = observation.TelescopeId,
            ResearcherId = observation.ResearcherId,
            ObservationDate = observation.ObservationDate,
            XrayFlux = observation.XrayFlux,
            Redshift = observation.Redshift
        };
    }
}
