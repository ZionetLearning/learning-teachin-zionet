namespace Manager.Services.Clients.Accessor.Models.Meetings;

/// <summary>
/// Response model received from Accessor service for generating ACS token
/// </summary>
public sealed record GenerateMeetingTokenAccessorResponse
{
    public required string UserId { get; init; }

    public required string Token { get; init; }

    public required DateTimeOffset ExpiresOn { get; init; }

    public required string GroupId { get; init; }
}
