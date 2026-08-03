using AstroTrack.Api.DTOs.CelestialObjects;

namespace AstroTrack.Api.Services;

public enum CelestialObjectMutationStatus
{
    Success,
    Duplicate,
    NotFound,
    ValidationFailed
}

public class CelestialObjectMutationResult
{
    public CelestialObjectMutationStatus Status { get; init; }

    public CelestialObjectDto? Data { get; init; }

    public string? Message { get; init; }

    public static CelestialObjectMutationResult Success(CelestialObjectDto? data = null)
    {
        return new CelestialObjectMutationResult
        {
            Status = CelestialObjectMutationStatus.Success,
            Data = data
        };
    }

    public static CelestialObjectMutationResult Duplicate(string message)
    {
        return new CelestialObjectMutationResult
        {
            Status = CelestialObjectMutationStatus.Duplicate,
            Message = message
        };
    }

    public static CelestialObjectMutationResult NotFound(string message)
    {
        return new CelestialObjectMutationResult
        {
            Status = CelestialObjectMutationStatus.NotFound,
            Message = message
        };
    }

    public static CelestialObjectMutationResult ValidationFailed(string message)
    {
        return new CelestialObjectMutationResult
        {
            Status = CelestialObjectMutationStatus.ValidationFailed,
            Message = message
        };
    }
}
