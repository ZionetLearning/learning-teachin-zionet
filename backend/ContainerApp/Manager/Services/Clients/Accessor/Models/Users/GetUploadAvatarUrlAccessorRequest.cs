namespace Manager.Services.Clients.Accessor.Models.Users;

public sealed record GetUploadAvatarUrlAccessorRequest
{
    public string ContentType { get; init; } = default!;
    public long? SizeBytes { get; init; }
    public string? ChecksumBase64 { get; init; }
}
