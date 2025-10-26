namespace Manager.Services.Avatars.Models;

public sealed class GetUploadUrlRequest
{
    public string ContentType { get; set; } = default!;
    public long? SizeBytes { get; set; }
    public string? ChecksumBase64 { get; set; }
}
