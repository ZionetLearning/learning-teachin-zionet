
using System.Text.Json.Serialization;

namespace Manager.Models.Users;

public class UserData
{
    public required Guid UserId { get; set; }
    public required string Email { get; set; }
    public required string FirstName { get; set; }
    public required string LastName { get; set; }

    [JsonConverter(typeof(JsonStringEnumConverter))]
    public required Role Role { get; set; }
    public SupportedLanguage PreferredLanguageCode { get; set; } = SupportedLanguage.en;
    public HebrewLevel? HebrewLevelValue { get; set; } // only for students
    public List<string> Interests { get; set; } = [];
}