namespace Manager.Services.Clients.Accessor.Models.Users;

public class ConfirmAvatarAccessorRequest
{
    public string BlobPath { get; set; } = default!;
    public string ContentType { get; set; } = default!;
    public int? Width { get; set; }
    public int? Height { get; set; }
    public long? SizeBytes { get; set; }
    public string? ETag { get; set; }
}
