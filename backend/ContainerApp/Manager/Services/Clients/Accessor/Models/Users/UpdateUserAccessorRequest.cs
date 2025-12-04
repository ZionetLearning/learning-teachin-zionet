using System.Text.Json.Serialization;
using Manager.Models.Users;

namespace Manager.Services.Clients.Accessor.Models.Users;

public sealed record UpdateUserAccessorRequest
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
