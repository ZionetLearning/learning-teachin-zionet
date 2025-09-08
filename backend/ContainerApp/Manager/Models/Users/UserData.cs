
namespace Manager.Models.Users;

public class UserData
{
    public required Guid UserId { get; set; }
    public required string Email { get; set; }
    public required string FirstName { get; set; }
    public required string LastName { get; set; }
    public required Role Role { get; set; }
}