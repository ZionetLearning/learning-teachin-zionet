namespace Accessor.Models;

public sealed class TaskWithEtag
{
    public TaskModel Task { get; init; } = default!;
    public string ETag { get; init; } = string.Empty;
}
