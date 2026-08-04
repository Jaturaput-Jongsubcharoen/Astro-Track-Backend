using AstroTrack.Api.DTOs.Missions;

namespace AstroTrack.Api.Services;

public enum MissionMutationStatus
{
    Success,
    Duplicate,
    NotFound,
    ValidationFailed
}

public class MissionMutationResult
{
    public MissionMutationStatus Status { get; init; }

    public MissionDto? Data { get; init; }

    public string? Message { get; init; }

    public static MissionMutationResult Success(MissionDto? data = null)
    {
        return new MissionMutationResult
        {
            Status = MissionMutationStatus.Success,
            Data = data
        };
    }

    public static MissionMutationResult Duplicate(string message)
    {
        return new MissionMutationResult
        {
            Status = MissionMutationStatus.Duplicate,
            Message = message
        };
    }

    public static MissionMutationResult NotFound(string message)
    {
        return new MissionMutationResult
        {
            Status = MissionMutationStatus.NotFound,
            Message = message
        };
    }

    public static MissionMutationResult ValidationFailed(string message)
    {
        return new MissionMutationResult
        {
            Status = MissionMutationStatus.ValidationFailed,
            Message = message
        };
    }
}
