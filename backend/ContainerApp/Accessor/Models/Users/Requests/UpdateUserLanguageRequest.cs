using System.Text.Json.Serialization;

namespace Accessor.Models.Users.Requests;

/// <summary>
/// Request DTO for updating user language preference
/// </summary>
public sealed record UpdateUserLanguageRequest
{
    public required Guid UserId { get; init; }

    [JsonConverter(typeof(JsonStringEnumConverter))]
    public required SupportedLanguage Language { get; init; }
}

