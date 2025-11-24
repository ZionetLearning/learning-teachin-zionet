using System.Text.Json.Serialization;

namespace Accessor.Models.Users;

public class UpdateUserModel
{
    public string? FirstName { get; set; }
    public string? LastName { get; set; }
    public SupportedLanguage? PreferredLanguageCode { get; set; }
    public HebrewLevel? HebrewLevelValue { get; set; }

    [JsonConverter(typeof(JsonStringEnumConverter))]
    public Role? Role { get; set; }
    public List<string>? Interests { get; set; }
    public string? AvatarPath { get; set; }
    public string? AvatarContentType { get; set; }
    public bool? ClearAvatar { get; set; }
    public string? AcsUserId { get; set; }
}
