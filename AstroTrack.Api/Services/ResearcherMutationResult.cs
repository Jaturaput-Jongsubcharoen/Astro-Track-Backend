using AstroTrack.Api.DTOs.Researchers;

namespace AstroTrack.Api.Services;

public enum ResearcherMutationStatus
{
    Success,
    Duplicate,
    NotFound,
    ValidationFailed
}

public class ResearcherMutationResult
{
    public ResearcherMutationStatus Status { get; init; }

    public ResearcherDto? Data { get; init; }

    public string? Message { get; init; }

    public static ResearcherMutationResult Success(ResearcherDto? data = null)
    {
        return new ResearcherMutationResult
        {
            Status = ResearcherMutationStatus.Success,
            Data = data
        };
    }

    public static ResearcherMutationResult Duplicate(string message)
    {
        return new ResearcherMutationResult
        {
            Status = ResearcherMutationStatus.Duplicate,
            Message = message
        };
    }

    public static ResearcherMutationResult NotFound(string message)
    {
        return new ResearcherMutationResult
        {
            Status = ResearcherMutationStatus.NotFound,
            Message = message
        };
    }

    public static ResearcherMutationResult ValidationFailed(string message)
    {
        return new ResearcherMutationResult
        {
            Status = ResearcherMutationStatus.ValidationFailed,
            Message = message
        };
    }
}