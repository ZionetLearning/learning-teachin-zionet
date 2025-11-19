namespace Manager.Services.Clients.Accessor.Models.Users;

public sealed record UnassignStudentAccessorRequest
{
    public Guid TeacherId { get; init; }
    public Guid StudentId { get; init; }
}
