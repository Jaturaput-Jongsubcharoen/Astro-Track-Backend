using AstroTrack.Api.DTOs.Researchers;
using AstroTrack.Api.Models;
using AstroTrack.Api.Repositories;
using Microsoft.EntityFrameworkCore;

namespace AstroTrack.Api.Services;

public class ResearcherService : IResearcherService
{
    private readonly IResearcherRepository _researcherRepository;
    private readonly ILogger<ResearcherService> _logger;

    public ResearcherService(
        IResearcherRepository researcherRepository,
        ILogger<ResearcherService> logger)
    {
        _researcherRepository = researcherRepository;
        _logger = logger;
    }

    public async Task<IEnumerable<ResearcherDto>> GetAllAsync()
    {
        var researchers = await _researcherRepository.GetAllAsync();
        return researchers.Select(MapToDto).ToList();
    }

    public async Task<ResearcherDto?> GetByIdAsync(long id)
    {
        var researcher = await _researcherRepository.GetByIdAsync(id);
        return researcher is null ? null : MapToDto(researcher);
    }

    public async Task<ResearcherMutationResult> CreateAsync(CreateResearcherDto dto)
    {
        if (await _researcherRepository.ExistsAsync(dto.ResearcherId))
        {
            return ResearcherMutationResult.Duplicate($"A researcher with ID {dto.ResearcherId} already exists.");
        }

        var entity = MapCreateDtoToEntity(dto);

        try
        {
            await _researcherRepository.AddAsync(entity);
            await _researcherRepository.SaveChangesAsync();
            return ResearcherMutationResult.Success(MapToDto(entity));
        }
        catch (DbUpdateException exception) when (IsDuplicateKeyViolation(exception))
        {
            _logger.LogWarning(exception, "Duplicate key violation while creating researcher with ID {ResearcherId}", dto.ResearcherId);
            return ResearcherMutationResult.Duplicate($"A researcher with ID {dto.ResearcherId} already exists.");
        }
        catch (DbUpdateException exception) when (IsConstraintViolation(exception))
        {
            _logger.LogWarning(exception, "Constraint violation while creating researcher with ID {ResearcherId}", dto.ResearcherId);
            return ResearcherMutationResult.ValidationFailed("The request violates one or more database constraints.");
        }
    }

    public async Task<ResearcherMutationResult> UpdateAsync(long id, UpdateResearcherDto dto)
    {
        var existingEntity = await _researcherRepository.GetByIdAsync(id);
        if (existingEntity is null)
        {
            return ResearcherMutationResult.NotFound($"Researcher with ID {id} was not found.");
        }

        ApplyUpdateDto(existingEntity, dto);

        try
        {
            await _researcherRepository.UpdateAsync(existingEntity);
            await _researcherRepository.SaveChangesAsync();
            return ResearcherMutationResult.Success(MapToDto(existingEntity));
        }
        catch (DbUpdateException exception) when (IsConstraintViolation(exception))
        {
            _logger.LogWarning(exception, "Constraint violation while updating researcher with ID {ResearcherId}", id);
            return ResearcherMutationResult.ValidationFailed("The request violates one or more database constraints.");
        }
    }

    public async Task<ResearcherMutationResult> DeleteAsync(long id)
    {
        var existingEntity = await _researcherRepository.GetByIdAsync(id);
        if (existingEntity is null)
        {
            return ResearcherMutationResult.NotFound($"Researcher with ID {id} was not found.");
        }

        try
        {
            await _researcherRepository.DeleteAsync(existingEntity);
            await _researcherRepository.SaveChangesAsync();
            return ResearcherMutationResult.Success();
        }
        catch (DbUpdateException exception) when (IsConstraintViolation(exception))
        {
            _logger.LogWarning(exception, "Constraint violation while deleting researcher with ID {ResearcherId}", id);
            return ResearcherMutationResult.ValidationFailed("The record cannot be deleted because of related data constraints.");
        }
    }

    private static Researcher MapCreateDtoToEntity(CreateResearcherDto dto)
    {
        return new Researcher
        {
            ResearcherId = dto.ResearcherId,
            ResearcherName = dto.ResearcherName,
            ContactEmail = string.IsNullOrWhiteSpace(dto.ContactEmail) ? null : dto.ContactEmail,
            PhoneNumber = string.IsNullOrWhiteSpace(dto.PhoneNumber) ? null : dto.PhoneNumber,
            AffiliationId = dto.AffiliationId
        };
    }

    private static void ApplyUpdateDto(Researcher entity, UpdateResearcherDto dto)
    {
        entity.ResearcherName = dto.ResearcherName;
        entity.ContactEmail = string.IsNullOrWhiteSpace(dto.ContactEmail) ? null : dto.ContactEmail;
        entity.PhoneNumber = string.IsNullOrWhiteSpace(dto.PhoneNumber) ? null : dto.PhoneNumber;
        entity.AffiliationId = dto.AffiliationId;
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

    private static ResearcherDto MapToDto(Researcher researcher)
    {
        return new ResearcherDto
        {
            ResearcherId = researcher.ResearcherId,
            ResearcherName = researcher.ResearcherName,
            ContactEmail = researcher.ContactEmail,
            PhoneNumber = researcher.PhoneNumber,
            AffiliationId = researcher.AffiliationId
        };
    }
}