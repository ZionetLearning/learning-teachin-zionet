namespace Manager.Models.Users;

public sealed record TeacherStudentMapDto
{
    public Guid TeacherId { get; init; }
    public Guid StudentId { get; init; }
}
