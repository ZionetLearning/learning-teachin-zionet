using System.Text.Json.Serialization;

namespace Accessor.Models.Users.Responses;

/// <summary>
/// Response DTO for getting a single user
/// </summary>
public sealed record GetUserResponse
{
    public required Guid UserId { get; init; }
    public required string Email { get; init; }
    public required string FirstName { get; init; }
    public required string LastName { get; init; }

    [JsonConverter(typeof(JsonStringEnumConverter))]
    public required Role Role { get; init; }

    [JsonConverter(typeof(JsonStringEnumConverter))]
    public SupportedLanguage PreferredLanguageCode { get; init; }

    [JsonConverter(typeof(JsonStringEnumConverter))]
    public HebrewLevel? HebrewLevelValue { get; init; }

    public List<string> Interests { get; init; } = [];
    public string? AcsUserId { get; init; }
    public string? AvatarPath { get; init; }
    public string? AvatarContentType { get; init; }
    public DateTime? AvatarUpdatedAtUtc { get; init; }
}

