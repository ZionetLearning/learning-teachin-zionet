namespace Accessor.Models.Classes.Responses;

/// <summary>
/// Response model for adding members to a class
/// </summary>
public sealed record AddMembersResponse
{
    public required bool Success { get; init; }
}

