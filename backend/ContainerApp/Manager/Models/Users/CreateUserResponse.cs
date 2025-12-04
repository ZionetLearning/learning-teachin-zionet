using System.Text.Json.Serialization;

namespace Manager.Models.Users;

public sealed record CreateUserResponse
{
    public Guid UserId { get; init; }
    public string Email { get; init; } = null!;
    public string FirstName { get; init; } = null!;
    public string LastName { get; init; } = null!;

    [JsonConverter(typeof(JsonStringEnumConverter))]
    public Role Role { get; init; }
}

