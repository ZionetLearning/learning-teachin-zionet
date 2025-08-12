namespace Accessor.Models;

public sealed record ThreadSummaryDto(
    Guid ThreadId, string ChatName, string ChatType,
    DateTimeOffset CreatedAt, DateTimeOffset UpdatedAt);
