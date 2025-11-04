namespace Engine.Models.Chat;

public class UserDetailForChat
{
    public required string FirstName { get; set; }
    public required string LastName { get; set; }
    public required string PreferredLanguageCode { get; set; }
    public required string? HebrewLevelValue { get; set; }
    public List<string>? Interests { get; set; }
    public required string? Role { get; set; }
}
