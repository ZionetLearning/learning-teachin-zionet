namespace Accessor.Models.Users;

public sealed record GetUploadAvatarUrlResponse
{
    public string UploadUrl { get; init; } = null!;
    public required string BlobPath { get; init; }
    public DateTime ExpiresAtUtc { get; init; }
    public long MaxBytes { get; init; }
    public string[] AcceptedContentTypes { get; init; } = [];
}
