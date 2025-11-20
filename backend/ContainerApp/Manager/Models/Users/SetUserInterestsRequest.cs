namespace Manager.Models.Users;

public sealed record SetUserInterestsRequest
{
    public required IReadOnlyList<string> Interests { get; init; }
}

