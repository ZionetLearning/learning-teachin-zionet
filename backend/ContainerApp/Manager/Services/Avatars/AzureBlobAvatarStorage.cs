using Azure;
using Azure.Storage.Blobs;
using Azure.Storage.Blobs.Models;
using Azure.Storage.Sas;
using Microsoft.Extensions.Options;

namespace Manager.Services.Avatars;

public sealed class AzureBlobAvatarStorage : IAvatarStorage
{
    private readonly AvatarsOptions _options;
    private readonly BlobServiceClient _svc;
    private readonly BlobContainerClient _container;

    public AzureBlobAvatarStorage(IOptions<AvatarsOptions> opt)
    {
        _options = opt.Value;
        _svc = new BlobServiceClient(_options.StorageConnectionString);
        _container = _svc.GetBlobContainerClient(_options.Container);
    }

    public async Task<(Uri uploadUrl, DateTimeOffset expiresAtUtc, string blobPath)>
        GetUploadSasAsync(Guid userId, string contentType, long? sizeBytes, CancellationToken ct)
    {
        // Валидации до выдачи SAS
        if (!_options.AllowedContentTypes.Contains(contentType, StringComparer.OrdinalIgnoreCase))
        {
            throw new InvalidOperationException($"Unsupported content-type: {contentType}");
        }

        if (sizeBytes is > 0 && sizeBytes > _options.MaxBytes)
        {
            throw new InvalidOperationException($"File is too large. Max {_options.MaxBytes} bytes.");
        }

        var now = DateTimeOffset.UtcNow;
        var expires = now.AddMinutes(_options.UploadUrlTtlMinutes);

        var ext = contentType switch
        {
            "image/jpeg" => "jpg",
            "image/png" => "png",
            "image/webp" => "webp",
            _ => "bin"
        };

        var blobPath = $"{userId}/avatar_v{DateTime.UtcNow.Ticks}.{ext}";
        var blob = _container.GetBlobClient(blobPath);

        await _container.CreateIfNotExistsAsync(PublicAccessType.None, cancellationToken: ct);

        var sasBuilder = new BlobSasBuilder
        {
            BlobContainerName = _container.Name,
            BlobName = blob.Name,
            Resource = "b",
            StartsOn = now.AddMinutes(-1),
            ExpiresOn = expires,
            Protocol = SasProtocol.HttpsAndHttp
        };
        sasBuilder.SetPermissions(BlobSasPermissions.Create | BlobSasPermissions.Write);

        var sasUri = blob.GenerateSasUri(sasBuilder);
        return (sasUri, expires, blobPath);
    }

    public async Task<bool> BlobExistsAsync(string blobPath, CancellationToken ct)
        => await _container.GetBlobClient(Relative(blobPath)).ExistsAsync(ct);

    public async Task<BlobProperties?> GetBlobPropsAsync(string blobPath, CancellationToken ct)
    {
        var blob = _container.GetBlobClient(Relative(blobPath));
        try
        {
            var resp = await blob.GetPropertiesAsync(cancellationToken: ct);
            return resp.Value;
        }
        catch (RequestFailedException ex) when (ex.Status == 404)
        {
            return null;
        }
    }

    public Task<Uri> GetReadSasAsync(string blobPath, TimeSpan ttl, CancellationToken ct)
    {
        var blob = _container.GetBlobClient(Relative(blobPath));

        var now = DateTimeOffset.UtcNow;
        var expires = now.Add(ttl);
        var b = new BlobSasBuilder
        {
            BlobContainerName = _container.Name,
            BlobName = blob.Name.Substring(blob.Name.IndexOf('/') + 1),
            Resource = "b",
            StartsOn = now.AddMinutes(-1),
            ExpiresOn = expires,
            Protocol = SasProtocol.HttpsAndHttp
        };
        b.SetPermissions(BlobSasPermissions.Read);
        var uri = blob.GenerateSasUri(b);
        return Task.FromResult(uri);
    }

    public Task DeleteAsync(string blobPath, CancellationToken ct)
        => _container.GetBlobClient(Relative(blobPath)).DeleteIfExistsAsync(DeleteSnapshotsOption.IncludeSnapshots, cancellationToken: ct);

    private static string Relative(string blobPath)
        // blobPath у нас вида "avatars/{userId}/..." — внутри контейнера путь тот же
        => blobPath.StartsWith("avatars/", StringComparison.Ordinal) ? blobPath : throw new ArgumentException("Invalid blobPath");
}
