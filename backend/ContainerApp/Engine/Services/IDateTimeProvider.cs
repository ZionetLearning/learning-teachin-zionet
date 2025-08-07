namespace Engine.Services;

public interface IDateTimeProvider
{
    DateTimeOffset UtcNow { get; }
}