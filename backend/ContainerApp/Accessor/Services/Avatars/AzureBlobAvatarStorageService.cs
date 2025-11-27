using Azure.Storage.Blobs;
using Azure.Storage.Blobs.Models;
using Azure.Storage.Sas;
using Accessor.Services.Avatars.Models;
using Microsoft.Extensions.Options;

namespace Accessor.Services.Avatars;

public sealed class AzureBlobAvatarStorageService : IAvatarStorageService
{
    private readonly AvatarsOptions _options;
    private readonly BlobServiceClient? _svc;
    private readonly BlobContainerClient? _container;
    private readonly ILogger<AzureBlobAvatarStorageService> _log;
    private Exception? _initError;

    public AzureBlobAvatarStorageService(IOptions<AvatarsOptions> opt, ILogger<AzureBlobAvatarStorageService> log)
    {

        _options = opt.Value;
        _log = log;

        var normConnection = _options.StorageConnectionString;

        _log.LogInformation("Avatar storage init. Container={Container}",
    _options.Container);

        var conn = _options.StorageConnectionString;

        try
        {
            _svc = new BlobServiceClient(normConnection);
            _container = _svc.GetBlobContainerClient(_options.Container);
            _log.LogInformation("Avatar storage init. Container={Container}", _options.Container);
        }
        catch (Exception ex)
        {
            _initError = ex;
            _log.LogError(ex,
                "Failed to init BlobServiceClient. ConnStr prefix={Prefix}",
                _options.StorageConnectionString?.Length > 20
                    ? _options.StorageConnectionString[..20] : _options.StorageConnectionString);
        }

        _log.LogInformation("Avatar storage init. Container={Container}, ConnStr={ConnStr}",
    _options.Container,
    _options.StorageConnectionString);
    }

    private void EnsureReady()
    {
        if (_initError != null)
        {
            throw new InvalidOperationException(
                "Avatar storage is misconfigured: invalid Storage connection string or container.",
                _initError);
        }

        if (_svc == null || _container == null)
        {
            throw new InvalidOperationException("Avatar storage is not initialized.");
        }
    }

    public async Task<(Uri uploadUrl, DateTimeOffset expiresAtUtc, string blobPath)>
        GetUploadSasAsync(Guid userId, string contentType, long? sizeBytes, CancellationToken ct)
    {
        EnsureReady();

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
        var blob = _container!.GetBlobClient(blobPath);

        try
        {
            // Ensure container exists
            await _container.CreateIfNotExistsAsync(PublicAccessType.None, cancellationToken: ct);

            var sasBuilder = new BlobSasBuilder
            {
                BlobContainerName = _options.Container,
                BlobName = blobPath,
                Resource = "b",
                StartsOn = now.AddMinutes(-1),
                ExpiresOn = expires
            };

            sasBuilder.SetPermissions(BlobSasPermissions.Write | BlobSasPermissions.Create);

            var sasUri = blob.GenerateSasUri(sasBuilder);

            _log.LogInformation("Generated SAS for {BlobPath}, expires {Expires}", blobPath, expires);

            return (sasUri, expires, blobPath);
        }
        catch (Exception ex)
        {
            _log.LogError(ex, "Failed to generate SAS for {BlobPath}", blobPath);
            throw;
        }
    }

    public async Task<bool> BlobExistsAsync(string blobPath, CancellationToken ct)
    {
        EnsureReady();
        var blob = _container!.GetBlobClient(blobPath);
        return await blob.ExistsAsync(ct);
    }

    public async Task<BlobProperties?> GetBlobPropsAsync(string blobPath, CancellationToken ct)
    {
        EnsureReady();
        var blob = _container!.GetBlobClient(blobPath);
        if (!await blob.ExistsAsync(ct))
        {
            return null;
        }

        var props = await blob.GetPropertiesAsync(cancellationToken: ct);
        return props.Value;
    }

    public Task<Uri> GenerateReadUrlAsync(string blobPath, TimeSpan ttl, CancellationToken ct)
    {
        EnsureReady();
        var blob = _container!.GetBlobClient(blobPath);

        var sasBuilder = new BlobSasBuilder
        {
            BlobContainerName = _options.Container,
            BlobName = blobPath,
            Resource = "b",
            StartsOn = DateTimeOffset.UtcNow.AddMinutes(-1),
            ExpiresOn = DateTimeOffset.UtcNow.Add(ttl)
        };
        sasBuilder.SetPermissions(BlobSasPermissions.Read);

        return Task.FromResult(blob.GenerateSasUri(sasBuilder));
    }

    public async Task DeleteAsync(string blobPath, CancellationToken ct)
    {
        EnsureReady();
        var blob = _container!.GetBlobClient(blobPath);
        await blob.DeleteIfExistsAsync(DeleteSnapshotsOption.IncludeSnapshots, cancellationToken: ct);
    }
}
