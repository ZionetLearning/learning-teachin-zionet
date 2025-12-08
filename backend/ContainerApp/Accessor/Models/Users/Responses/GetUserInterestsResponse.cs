namespace Accessor.Models.Users.Responses;

/// <summary>
/// Response DTO for getting user interests
/// </summary>
public sealed record GetUserInterestsResponse
{
    public required List<string> Interests { get; init; }
}

