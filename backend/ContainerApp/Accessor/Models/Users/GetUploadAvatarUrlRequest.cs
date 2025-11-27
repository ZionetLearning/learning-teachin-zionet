namespace Accessor.Models.Users;

public sealed record GetUploadAvatarUrlRequest
{
    public string ContentType { get; init; } = default!;
    public long? SizeBytes { get; init; }
    public string? ChecksumBase64 { get; init; }
}
