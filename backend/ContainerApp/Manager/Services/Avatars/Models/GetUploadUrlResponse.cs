namespace Manager.Services.Avatars.Models;

public sealed class AvatarUploadUrlResponse
{
    public string UploadUrl { get; set; } = default!;
    public string BlobPath { get; set; } = default!;
    public DateTime ExpiresAtUtc { get; set; }
    public long MaxBytes { get; set; }
    public string[] AcceptedContentTypes { get; set; } = Array.Empty<string>();
}
