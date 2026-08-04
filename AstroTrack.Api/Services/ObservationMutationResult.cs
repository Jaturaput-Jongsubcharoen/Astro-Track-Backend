using AstroTrack.Api.DTOs.Observations;

namespace AstroTrack.Api.Services;

public enum ObservationMutationStatus
{
    Success,
    Duplicate,
    NotFound,
    ValidationFailed
}

public class ObservationMutationResult
{
    public ObservationMutationStatus Status { get; init; }

    public ObservationDto? Data { get; init; }

    public string? Message { get; init; }

    public static ObservationMutationResult Success(ObservationDto? data = null)
    {
        return new ObservationMutationResult
        {
            Status = ObservationMutationStatus.Success,
            Data = data
        };
    }

    public static ObservationMutationResult Duplicate(string message)
    {
        return new ObservationMutationResult
        {
            Status = ObservationMutationStatus.Duplicate,
            Message = message
        };
    }

    public static ObservationMutationResult NotFound(string message)
    {
        return new ObservationMutationResult
        {
            Status = ObservationMutationStatus.NotFound,
            Message = message
        };
    }

    public static ObservationMutationResult ValidationFailed(string message)
    {
        return new ObservationMutationResult
        {
            Status = ObservationMutationStatus.ValidationFailed,
            Message = message
        };
    }
}
