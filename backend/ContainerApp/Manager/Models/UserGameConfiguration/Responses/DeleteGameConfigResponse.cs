namespace Manager.Models.UserGameConfiguration.Responses;

/// <summary>
/// Response model for deleting game configuration
/// </summary>
public sealed record DeleteGameConfigResponse
{
    public required string Message { get; init; }
}
