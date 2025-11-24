namespace Manager.Models.UserGameConfiguration.Responses;

/// <summary>
/// Response model for saving game configuration
/// </summary>
public sealed record SaveGameConfigResponse
{
    public required string Message { get; init; }
}
