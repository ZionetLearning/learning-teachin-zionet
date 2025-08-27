namespace Accessor.Models;

public sealed record ChatSummaryDto(
    Guid ChatId, string ChatName, string ChatType,
    DateTimeOffset CreatedAt, DateTimeOffset UpdatedAt);
