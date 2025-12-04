using System.Text.Json.Serialization;
using Manager.Models.Users;

namespace Manager.Services.Clients.Accessor.Models.Users;

public sealed record CreateUserAccessorRequest
{
    public Guid UserId { get; init; }
    public required string FirstName { get; init; }
    public required string LastName { get; init; }
    public required string Email { get; init; }
    public required string Password { get; init; }

    [JsonConverter(typeof(JsonStringEnumConverter))]
    public Role Role { get; init; }
    public SupportedLanguage PreferredLanguageCode { get; init; } = SupportedLanguage.en;
    public HebrewLevel? HebrewLevelValue { get; init; } // only for students
    public IReadOnlyList<string> Interests { get; init; } = [];
}
