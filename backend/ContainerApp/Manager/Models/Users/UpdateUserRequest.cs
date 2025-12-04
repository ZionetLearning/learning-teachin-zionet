using System.Text.Json.Serialization;

namespace Manager.Models.Users;

public sealed record UpdateUserRequest
{
    public string? FirstName { get; init; }
    public string? LastName { get; init; }
    public string? Email { get; init; }
    public SupportedLanguage? PreferredLanguageCode { get; init; }
    public HebrewLevel? HebrewLevelValue { get; init; }

    [JsonConverter(typeof(JsonStringEnumConverter))]
    public Role? Role { get; init; }
    public IReadOnlyList<string>? Interests { get; init; }
    public string? AcsUserId { get; init; }
    public string? AvatarPath { get; init; }
    public string? AvatarContentType { get; init; }
    public bool? ClearAvatar { get; init; }
}
