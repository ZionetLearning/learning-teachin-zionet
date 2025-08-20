namespace Accessor.Services;

public interface IDateTimeProvider
{
    DateTimeOffset UtcNow { get; }
}

