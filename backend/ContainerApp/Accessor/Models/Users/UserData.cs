using System.Text.Json.Serialization;

namespace Accessor.Models.Users;

public class UserData
{
    public required Guid UserId { get; set; }
    public required string Email { get; set; }
    public required string FirstName { get; set; }
    public required string LastName { get; set; }

    [JsonConverter(typeof(JsonStringEnumConverter))]
    public required Role Role { get; set; }
    public SupportedLanguage PreferredLanguageCode { get; set; }
    public HebrewLevel? HebrewLevelValue { get; set; }
    public List<string> Interests { get; set; } = [];
    public string? AvatarPath { get; set; }
    public string? AvatarContentType { get; set; }
    public DateTime? AvatarUpdatedAtUtc { get; set; }
    public string? AcsUserId { get; set; }
}
