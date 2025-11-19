namespace Manager.Models.Users;

public sealed record GetUploadAvatarUrlResponse
{
    public string UploadUrl { get; init; } = null!;
    public string BlobPath { get; init; } = null!;
    public DateTime ExpiresAtUtc { get; init; }
    public long MaxBytes { get; init; }
    public string[] AcceptedContentTypes { get; init; } = [];
}
