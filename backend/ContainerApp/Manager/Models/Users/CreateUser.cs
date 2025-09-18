
namespace Manager.Models.Users;

public class CreateUser
{
    public Guid UserId { get; set; }
    public required string FirstName { get; set; }
    public required string LastName { get; set; }
    public required string Email { get; set; }
    public required string Password { get; set; }
    public required string Role { get; set; }

    // Interests is only for students
    public List<string> Interests { get; set; } = [];
}
