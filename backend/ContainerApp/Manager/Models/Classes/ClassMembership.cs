using Manager.Models.Users;

namespace Manager.Models.Classes;

public class ClassMembership
{
    public Guid ClassId { get; set; }
    public Guid UserId { get; set; }

    public Role Role { get; set; }
    public Guid AddedBy { get; set; }
    public DateTime AddedAt { get; set; } = DateTime.UtcNow;
}
public record MemberDto
{
    public Guid MemberId { get; set; }
    public required string Name { get; set; }
}