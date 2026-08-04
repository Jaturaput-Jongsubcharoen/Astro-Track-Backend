using AstroTrack.Api.DTOs.Observations;

namespace AstroTrack.Api.Services;

public interface IObservationService
{
    Task<IEnumerable<ObservationDto>> GetAllAsync();

    Task<ObservationDto?> GetByIdAsync(long id);

    Task<ObservationMutationResult> CreateAsync(CreateObservationDto dto);

    Task<ObservationMutationResult> UpdateAsync(long id, UpdateObservationDto dto);

    Task<ObservationMutationResult> DeleteAsync(long id);
}
