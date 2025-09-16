namespace IntegrationTests.Models
{
    public sealed class TaskWithEtagDto
    {
        public TaskModel Task { get; init; } = default!;
        public string ETag { get; init; } = string.Empty;
    }

}
