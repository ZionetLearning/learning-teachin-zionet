namespace Accessor.Services.Interfaces;

public interface IDateTimeProvider
{
    DateTimeOffset UtcNow { get; }
}

