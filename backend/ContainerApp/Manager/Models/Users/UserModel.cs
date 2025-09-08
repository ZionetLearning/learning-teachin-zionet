using System.Text.Json.Serialization;

namespace Manager.Models.Users;

public class UserModel
{
    public Guid UserId { get; set; }
    public required string FirstName { get; set; }
    public required string LastName { get; set; }
    public required string Email { get; set; }
    public required string Password { get; set; }

    [JsonConverter(typeof(JsonStringEnumConverter))]
    public Role Role { get; set; }
    public SupportedLanguage PreferredLanguageCode { get; set; } = SupportedLanguage.en;
    public HebrewLevel? HebrewLevelValue { get; set; } // only for students
}
