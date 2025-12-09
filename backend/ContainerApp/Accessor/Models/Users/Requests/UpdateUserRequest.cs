using System.Text.Json.Serialization;

namespace Accessor.Models.Users.Requests;

/// <summary>
/// Request DTO for updating a user
/// </summary>
public sealed record UpdateUserRequest
{
    public string? FirstName { get; init; }
    public string? LastName { get; init; }

    [JsonConverter(typeof(JsonStringEnumConverter))]
    public SupportedLanguage? PreferredLanguageCode { get; init; }

    [JsonConverter(typeof(JsonStringEnumConverter))]
    public HebrewLevel? HebrewLevelValue { get; init; }

    [JsonConverter(typeof(JsonStringEnumConverter))]
    public Role? Role { get; init; }

    public List<string>? Interests { get; init; }
    public string? AvatarPath { get; init; }
    public string? AvatarContentType { get; init; }
    public bool? ClearAvatar { get; init; }
    public string? AcsUserId { get; init; }
}

