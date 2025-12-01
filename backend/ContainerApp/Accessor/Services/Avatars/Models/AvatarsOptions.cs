namespace Accessor.Services.Avatars.Models;

public sealed class AvatarsOptions
{
    public const string SectionName = "Avatars";
    public string StorageConnectionString { get; set; } = default!;
    public string Container { get; set; } = "avatars";
    public int UploadUrlTtlMinutes { get; set; } = 5;
    public int ReadUrlTtlMinutes { get; set; } = 15;
    public long MaxBytes { get; set; } = 2 * 1024 * 1024;
    public string[] AllowedContentTypes { get; set; } = new[] { "image/jpeg", "image/png", "image/webp" };
}
