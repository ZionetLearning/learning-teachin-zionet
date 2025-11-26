namespace Manager.Services.Clients.Accessor.Models.Meetings;

/// <summary>
/// Response model received from Accessor service for ACS identity
/// </summary>
public sealed record CreateOrGetIdentityAccessorResponse
{
    public required string AcsUserId { get; init; }
}
