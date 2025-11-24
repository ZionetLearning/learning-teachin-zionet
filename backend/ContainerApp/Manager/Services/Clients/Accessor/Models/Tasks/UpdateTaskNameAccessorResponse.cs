namespace Manager.Services.Clients.Accessor.Models.Tasks;

/// <summary>
/// Response model received from Accessor service after updating a task name
/// </summary>
public sealed record UpdateTaskNameAccessorResponse
{
    public required bool Updated { get; init; }
    public required bool NotFound { get; init; }
    public required bool PreconditionFailed { get; init; }
    public string? NewEtag { get; init; }
}
