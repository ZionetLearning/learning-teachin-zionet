using Azure.Storage.Blobs.Models;

namespace Manager.Services.Avatars;

public interface IAvatarStorage
{
    Task<(Uri uploadUrl, DateTimeOffset expiresAtUtc, string blobPath)> GetUploadSasAsync(Guid userId, string contentType, long? sizeBytes, CancellationToken ct);

    Task<bool> BlobExistsAsync(string blobPath, CancellationToken ct);

    Task<BlobProperties?> GetBlobPropsAsync(string blobPath, CancellationToken ct);

    Task<Uri> GetReadSasAsync(string blobPath, TimeSpan ttl, CancellationToken ct);

    Task DeleteAsync(string blobPath, CancellationToken ct);
}