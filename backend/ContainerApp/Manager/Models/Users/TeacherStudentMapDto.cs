namespace Manager.Models.Users;

public sealed class TeacherStudentMapDto
{
    public Guid TeacherId { get; init; }
    public Guid StudentId { get; init; }
}
