namespace Manager.Models;

public record StatsSnapshot(
    long TotalThreads,
    long TotalUniqueUsersByThread,
    long TotalMessages,
    long TotalUniqueUsersByMessage,
    long ActiveUsersLast15m,
    long MessagesLast5m,
    long MessagesLast15m,
    DateTimeOffset GeneratedAtUtc
);
