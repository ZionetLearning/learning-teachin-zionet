using Azure;
using Azure.Storage.Blobs;
using Azure.Storage.Blobs.Models;
using Azure.Storage.Sas;
using Manager.Services.Avatars.Models;
using Microsoft.Extensions.Options;

namespace Manager.Services.Avatars;

public sealed class AzureBlobAvatarStorage : IAvatarStorage
{
    private readonly AvatarsOptions _options;
    private readonly BlobServiceClient _svc;
    private readonly BlobContainerClient _container;
    private readonly ILogger<AzureBlobAvatarStorage> _log;

    public AzureBlobAvatarStorage(IOptions<AvatarsOptions> opt, ILogger<AzureBlobAvatarStorage> log)
    {
        _options = opt.Value;
        _svc = new BlobServiceClient(_options.StorageConnectionString);
        _container = _svc.GetBlobContainerClient(_options.Container);
        _log = log;

        _log.LogInformation("Avatar storage init. Container={Container}", _options.Container);
    }

    public async Task<(Uri uploadUrl, DateTimeOffset expiresAtUtc, string blobPath)>
        GetUploadSasAsync(Guid userId, string contentType, long? sizeBytes, CancellationToken ct)
    {
        using var _ = _log.BeginScope("GetUploadSas userId={UserId}", userId);

        if (!_options.AllowedContentTypes.Contains(contentType, StringComparer.OrdinalIgnoreCase))
        {
            _log.LogWarning("Unsupported content-type: {CT}", contentType);
            throw new InvalidOperationException($"Unsupported content-type: {contentType}");
        }

        if (sizeBytes is > 0 && sizeBytes > _options.MaxBytes)
        {
            _log.LogWarning("Too large upload requested: {Size} > {Max}", sizeBytes, _options.MaxBytes);
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

        try
        {
            await _container.CreateIfNotExistsAsync(PublicAccessType.None, cancellationToken: ct);

        }
        catch (RequestFailedException ex)
        {
            _log.LogError(ex, "Failed to ensure container exists or set access policy");
            throw;
        }

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
        _log.LogInformation("Upload SAS issued. BlobPath={BlobPath}, TTLmin={TTL}", blobPath, _options.UploadUrlTtlMinutes);
        return (sasUri, expires, blobPath);
    }

    public async Task<bool> BlobExistsAsync(string blobPath, CancellationToken ct)
        => await _container.GetBlobClient(Relative(blobPath)).ExistsAsync(ct);

    public async Task<BlobProperties?> GetBlobPropsAsync(string blobPath, CancellationToken ct)
    {
        using var _ = _log.BeginScope("GetBlobProps path={Path}", blobPath);
        var blob = _container.GetBlobClient(Relative(blobPath));
        try
        {
            var response = await blob.GetPropertiesAsync(cancellationToken: ct);
            var p = response.Value;
            _log.LogInformation("Props: Len={Len}, CT={CT}, ETag={ETag}", p.ContentLength, p.ContentType, p.ETag);
            return response.Value;
        }
        catch (RequestFailedException ex) when (ex.Status == 404)
        {
            _log.LogWarning("Blob not found (404)");
            return null;
        }

        catch (RequestFailedException ex)
        {
            _log.LogError(ex, "GetProperties failed");
            throw;
        }
    }

    public Task<Uri> GetReadSasAsync(string blobPath, TimeSpan ttl, CancellationToken ct)
    {
        using var _ = _log.BeginScope("GetReadSas path={Path}", blobPath);

        var blob = _container.GetBlobClient(Relative(blobPath));

        var now = DateTimeOffset.UtcNow;
        var expires = now.Add(ttl);
        var b = new BlobSasBuilder
        {
            BlobContainerName = _container.Name,
            Resource = "b",
            StartsOn = now.AddMinutes(-1),
            ExpiresOn = expires,
            Protocol = SasProtocol.HttpsAndHttp
        };
        b.SetPermissions(BlobSasPermissions.Read);
        var uri = blob.GenerateSasUri(b);
        _log.LogInformation("Read SAS issued. TTLmin={TTL}", ttl.TotalMinutes);

        return Task.FromResult(uri);
    }

    public Task DeleteAsync(string blobPath, CancellationToken ct)
        => _container.GetBlobClient(Relative(blobPath)).DeleteIfExistsAsync(DeleteSnapshotsOption.IncludeSnapshots, cancellationToken: ct);

    private static string Relative(string blobPath)
        => !string.IsNullOrWhiteSpace(blobPath)
            ? blobPath
            : throw new ArgumentException("Invalid blobPath");
}
