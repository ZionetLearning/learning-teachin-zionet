using System.Text.Json.Serialization;

namespace Accessor.Models.Users.Requests;

/// <summary>
/// Request DTO for creating a new user
/// </summary>
public sealed record CreateUserRequest
{
    public required Guid UserId { get; init; }
    public required string FirstName { get; init; }
    public required string LastName { get; init; }
    public required string Email { get; init; }
    public required string Password { get; init; }

    [JsonConverter(typeof(JsonStringEnumConverter))]
    public required Role Role { get; init; }

    [JsonConverter(typeof(JsonStringEnumConverter))]
    public required SupportedLanguage PreferredLanguageCode { get; init; }

    [JsonConverter(typeof(JsonStringEnumConverter))]
    public HebrewLevel? HebrewLevelValue { get; init; }

    public required List<string> Interests { get; init; }
}

