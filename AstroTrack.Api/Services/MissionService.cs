using AstroTrack.Api.DTOs.Missions;
using AstroTrack.Api.Models;
using AstroTrack.Api.Repositories;
using Microsoft.EntityFrameworkCore;

namespace AstroTrack.Api.Services;

public class MissionService : IMissionService
{
    private readonly IMissionRepository _missionRepository;
    private readonly ILogger<MissionService> _logger;

    public MissionService(
        IMissionRepository missionRepository,
        ILogger<MissionService> logger)
    {
        _missionRepository = missionRepository;
        _logger = logger;
    }

    public async Task<IEnumerable<MissionDto>> GetAllAsync()
    {
        var missions = await _missionRepository.GetAllAsync();
        return missions.Select(MapToDto).ToList();
    }

    public async Task<MissionDto?> GetByIdAsync(long id)
    {
        var mission = await _missionRepository.GetByIdAsync(id);
        return mission is null ? null : MapToDto(mission);
    }

    public async Task<MissionMutationResult> CreateAsync(CreateMissionDto dto)
    {
        if (await _missionRepository.ExistsAsync(dto.MissionId))
        {
            return MissionMutationResult.Duplicate($"A mission with ID {dto.MissionId} already exists.");
        }

        if (!HasValidDates(dto.StartDate, dto.EndDate))
        {
            return MissionMutationResult.ValidationFailed("EndDate must be greater than or equal to StartDate.");
        }

        var entity = MapCreateDtoToEntity(dto);

        try
        {
            await _missionRepository.AddAsync(entity);
            await _missionRepository.SaveChangesAsync();
            return MissionMutationResult.Success(MapToDto(entity));
        }
        catch (DbUpdateException exception) when (IsDuplicateKeyViolation(exception))
        {
            _logger.LogWarning(exception, "Duplicate key violation while creating mission with ID {MissionId}", dto.MissionId);
            return MissionMutationResult.Duplicate($"A mission with ID {dto.MissionId} already exists.");
        }
        catch (DbUpdateException exception) when (IsConstraintViolation(exception))
        {
            _logger.LogWarning(exception, "Constraint violation while creating mission with ID {MissionId}", dto.MissionId);
            return MissionMutationResult.ValidationFailed("The request violates one or more database constraints.");
        }
    }

    public async Task<MissionMutationResult> UpdateAsync(long id, UpdateMissionDto dto)
    {
        var existingEntity = await _missionRepository.GetByIdAsync(id);
        if (existingEntity is null)
        {
            return MissionMutationResult.NotFound($"Mission with ID {id} was not found.");
        }

        if (!HasValidDates(dto.StartDate, dto.EndDate))
        {
            return MissionMutationResult.ValidationFailed("EndDate must be greater than or equal to StartDate.");
        }

        ApplyUpdateDto(existingEntity, dto);

        try
        {
            await _missionRepository.UpdateAsync(existingEntity);
            await _missionRepository.SaveChangesAsync();
            return MissionMutationResult.Success(MapToDto(existingEntity));
        }
        catch (DbUpdateException exception) when (IsConstraintViolation(exception))
        {
            _logger.LogWarning(exception, "Constraint violation while updating mission with ID {MissionId}", id);
            return MissionMutationResult.ValidationFailed("The request violates one or more database constraints.");
        }
    }

    public async Task<MissionMutationResult> DeleteAsync(long id)
    {
        var existingEntity = await _missionRepository.GetByIdAsync(id);
        if (existingEntity is null)
        {
            return MissionMutationResult.NotFound($"Mission with ID {id} was not found.");
        }

        try
        {
            await _missionRepository.DeleteAsync(existingEntity);
            await _missionRepository.SaveChangesAsync();
            return MissionMutationResult.Success();
        }
        catch (DbUpdateException exception) when (IsConstraintViolation(exception))
        {
            _logger.LogWarning(exception, "Constraint violation while deleting mission with ID {MissionId}", id);
            return MissionMutationResult.ValidationFailed("The record cannot be deleted because of related data constraints.");
        }
    }

    private static Mission MapCreateDtoToEntity(CreateMissionDto dto)
    {
        return new Mission
        {
            MissionId = dto.MissionId,
            MissionName = dto.MissionName,
            MissionPurpose = dto.MissionPurpose,
            StartDate = dto.StartDate,
            EndDate = dto.EndDate,
            LeadResearcherId = dto.LeadResearcherId,
            AffiliationId = dto.AffiliationId
        };
    }

    private static void ApplyUpdateDto(Mission entity, UpdateMissionDto dto)
    {
        entity.MissionName = dto.MissionName;
        entity.MissionPurpose = dto.MissionPurpose;
        entity.StartDate = dto.StartDate;
        entity.EndDate = dto.EndDate;
        entity.LeadResearcherId = dto.LeadResearcherId;
        entity.AffiliationId = dto.AffiliationId;
    }

    private static bool HasValidDates(DateTime startDate, DateTime? endDate)
    {
        return endDate is null || endDate.Value >= startDate;
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

    private static MissionDto MapToDto(Mission mission)
    {
        return new MissionDto
        {
            MissionId = mission.MissionId,
            MissionName = mission.MissionName,
            MissionPurpose = mission.MissionPurpose,
            StartDate = mission.StartDate,
            EndDate = mission.EndDate,
            LeadResearcherId = mission.LeadResearcherId,
            AffiliationId = mission.AffiliationId
        };
    }
}
