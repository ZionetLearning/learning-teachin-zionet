using Azure;
using Azure.Storage.Blobs;
using Azure.Storage.Blobs.Models;
using Azure.Storage.Sas;
using Manager.Services.Avatars.Models;
using Microsoft.Extensions.Options;

namespace Manager.Services.Avatars;

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

        var raw = _options.StorageConnectionString;
        var norm = NormalizeConnString(raw);

        _log.LogInformation("Avatar storage init. Container={Container}, ConnStr={ConnStr}",
    _options.Container,
    norm);

        var conn = _options.StorageConnectionString;

        try
        {
            _svc = new BlobServiceClient(norm);
            _container = _svc.GetBlobContainerClient(_options.Container);
            _log.LogInformation("Avatar storage init. Container={Container}", _options.Container);
        }
        catch (FormatException fe)
        {
            // Добавь деталь для отладки
            _log.LogWarning("ConnStr length={Len}, startsQuote={StartQ}, endsQuote={EndQ}",
                norm.Length, norm.StartsWith('"') || norm.StartsWith('\''), norm.EndsWith('"') || norm.EndsWith('\''));

            // Ещё можно подсветить первые N символов в hex (без ключа!)
            _log.LogWarning("ConnStr prefix hex: {Hex}",
                string.Join(" ", norm.Take(48).Select(ch => ((int)ch).ToString("X2"))));

            throw new InvalidOperationException(
                "Avatar storage is misconfigured: invalid Storage connection string or container.", fe);
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
    {
        EnsureReady();
        return await _container!.GetBlobClient(Relative(blobPath)).ExistsAsync(ct);
    }

    public async Task<BlobProperties?> GetBlobPropsAsync(string blobPath, CancellationToken ct)
    {
        EnsureReady();

        using var _ = _log.BeginScope("GetBlobProps path={Path}", blobPath);
        var blob = _container!.GetBlobClient(Relative(blobPath));
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

    public Task<Uri> GenerateReadUrlAsync(string blobPath, TimeSpan ttl, CancellationToken ct)
    {
        EnsureReady();

        using var _ = _log.BeginScope("GetReadSas path={Path}", blobPath);

        var blob = _container!.GetBlobClient(Relative(blobPath));

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
    {
        EnsureReady();

        return _container!
            .GetBlobClient(Relative(blobPath))
            .DeleteIfExistsAsync(DeleteSnapshotsOption.IncludeSnapshots, cancellationToken: ct);
    }

    private static string Relative(string blobPath)
        => !string.IsNullOrWhiteSpace(blobPath)
            ? blobPath
            : throw new ArgumentException("Invalid blobPath");

    private static string NormalizeConnString(string? cs)
    {
        if (string.IsNullOrWhiteSpace(cs))
        {
            return cs ?? "";
        }

        var charsToDrop = new[] { '\uFEFF', '\u200B', '\u200C', '\u200D', '\u200E', '\u200F', '\u2060', '\u00A0' };
        var cleaned = new string(cs.Where(c => !charsToDrop.Contains(c)).ToArray());

        cleaned = cleaned.Trim().Trim('\"', '\'');

        return cleaned;
    }
}
