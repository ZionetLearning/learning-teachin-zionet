using Accessor.Models.Users;

namespace Accessor.Models.Auth;

public class AuthenticatedUser
{
    public Guid UserId { get; set; }
    public Role Role { get; set; }
}

