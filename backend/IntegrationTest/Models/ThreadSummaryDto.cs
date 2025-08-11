// IntegrationTests.Models/ThreadSummaryDto.cs
namespace IntegrationTests.Models;

public sealed class ThreadSummaryDto
{
    public Guid ThreadId { get; set; }
    public string ChatName { get; set; } = "";
    public string ChatType { get; set; } = "default";
    public DateTimeOffset CreatedAt { get; set; }
    public DateTimeOffset UpdatedAt { get; set; }
}
