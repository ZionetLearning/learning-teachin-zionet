using Manager.Models.Users;

namespace Manager.Models.Auth;

public class AuthenticatedUser
{
    public Guid UserId { get; set; }
    public Role Role { get; set; }
}

