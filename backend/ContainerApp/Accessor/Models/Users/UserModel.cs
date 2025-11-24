using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json.Serialization;

namespace Accessor.Models.Users;

[Table("Users")]
public class UserModel
{
    [Key]
    public Guid UserId { get; set; }
    public required string FirstName { get; set; }
    public required string LastName { get; set; }
    public required string Email { get; set; }
    public required string Password { get; set; }

    [JsonConverter(typeof(JsonStringEnumConverter))]
    public required Role Role { get; set; }

    [JsonConverter(typeof(JsonStringEnumConverter))]
    public SupportedLanguage PreferredLanguageCode { get; set; }

    [JsonConverter(typeof(JsonStringEnumConverter))]
    public HebrewLevel? HebrewLevelValue { get; set; }
    public required List<string> Interests { get; set; }
    public string? AvatarPath { get; set; }
    public string? AvatarContentType { get; set; }
    public DateTime? AvatarUpdatedAtUtc { get; set; }

    public string? AcsUserId { get; set; }
}
