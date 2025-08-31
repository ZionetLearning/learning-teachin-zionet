using Microsoft.AspNetCore.ResponseCompression;

namespace Manager.Constants;

internal static class CompressionDefaults
{
    internal static readonly string[] CompressedMimeTypes =
        ResponseCompressionDefaults.MimeTypes
            .Concat(new[] { "application/json", "application/problem+json" })
            .ToArray();
}
