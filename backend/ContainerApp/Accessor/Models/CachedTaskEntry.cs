namespace Accessor.Models;
public class CachedTaskEntry
{
    public TaskModel Task { get; set; } = default!;
    public string ETag { get; set; } = string.Empty;
}