namespace Accessor.Functions.Models;

public record DatabaseConfiguration
{
    public string Host { get; init; } = string.Empty;
    public string Database { get; init; } = string.Empty;
    public string Username { get; init; } = string.Empty;
    public string Password { get; init; } = string.Empty;
}