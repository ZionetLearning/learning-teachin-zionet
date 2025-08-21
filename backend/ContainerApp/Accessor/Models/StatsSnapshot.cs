namespace Accessor.Models;

public record StatsSnapshot(
    long TotalUsers,
    long TotalThreads,
    long TotalMessages,
    DateTimeOffset GeneratedAtUtc
);
