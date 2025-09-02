using Microsoft.AspNetCore.ResponseCompression;

namespace Manager.Constants;

internal static class CompressionDefaults
{
    private static readonly string[] ExtraMimeTypes =
    {
        "application/json",
        "application/problem+json",
        "audio/wav",
        "audio/x-wav",
        "application/octet-stream"
    };

    internal static readonly string[] CompressedMimeTypes =
        ResponseCompressionDefaults.MimeTypes
            .Concat(ExtraMimeTypes)
            .ToArray();
}
