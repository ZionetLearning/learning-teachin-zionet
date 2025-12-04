using System.Text.Json.Serialization;
using Manager.Models.Users;

namespace Manager.Services.Clients.Accessor.Models.Users;

public sealed record GetUserAccessorResponse
{
    public required Guid UserId { get; init; }
    public required string Email { get; init; }
    public required string FirstName { get; init; }
    public required string LastName { get; init; }

    [JsonConverter(typeof(JsonStringEnumConverter))]
    public required Role Role { get; init; }
    public SupportedLanguage PreferredLanguageCode { get; init; } = SupportedLanguage.en;
    public HebrewLevel? HebrewLevelValue { get; init; } // only for students
    public List<string>? Interests { get; init; } // only for students
    public string? AcsUserId { get; init; }
    public string? AvatarPath { get; init; }
    public string? AvatarContentType { get; init; }
    public DateTime? AvatarUpdatedAtUtc { get; init; }
}
