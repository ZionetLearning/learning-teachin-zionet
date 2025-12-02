namespace Accessor.Models.Classes.Responses;

/// <summary>
/// Response model for removing members from a class
/// </summary>
public sealed record RemoveMembersResponse
{
    public required bool Success { get; init; }
}

