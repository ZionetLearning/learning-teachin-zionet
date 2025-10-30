using Accessor.Models.Users;

namespace Accessor.Models.Classes;

public class ClassMembership
{
    public Guid ClassId { get; set; }
    public Guid UserId { get; set; }

    public Role Role { get; set; }
    public Guid AddedBy { get; set; }
    public DateTime AddedAt { get; set; } = DateTime.UtcNow;
    public Class Class { get; set; } = null!;
    public UserModel User { get; set; } = null!;
}

public record MemberDto
{
    public Guid MemberId { get; set; }
    public required string Name { get; set; }
}
