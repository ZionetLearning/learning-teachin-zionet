
namespace Accessor.Models.Users;

public class UpdateUserModel
{
    public required string Email { get; set; }
    public required string PasswordHash { get; set; }
}
