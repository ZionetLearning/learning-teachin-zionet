namespace Manager.Models.Classes.Requests;

/// <summary>
/// Request model for adding members to a class
/// </summary>
public sealed record AddMembersRequest
{
    public required IReadOnlyList<Guid> UserIds { get; init; }
    public required Guid AddedBy { get; init; }
}
