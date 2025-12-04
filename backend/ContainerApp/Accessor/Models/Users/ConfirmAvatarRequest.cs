namespace Accessor.Models.Users;

public class ConfirmAvatarRequest
{
    public string BlobPath { get; set; } = default!;
    public string ContentType { get; set; } = default!;
    public int? Width { get; set; }
    public int? Height { get; set; }
    public long? SizeBytes { get; set; }
    public string? ETag { get; set; }
}
