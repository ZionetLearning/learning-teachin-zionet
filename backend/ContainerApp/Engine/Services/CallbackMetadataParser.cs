namespace Engine.Services;

public static class CallbackMetadataParser
{
    public static (string Queue, string Method)? TryParse(IReadOnlyDictionary<string, string>? metadata)
    {
        if (metadata == null)
        {
            return null;
        }

        if (metadata.TryGetValue(CallbackHeaderHelper.HeaderQueue, out var queue) &&
            metadata.TryGetValue(CallbackHeaderHelper.HeaderMethod, out var method) &&
            !string.IsNullOrWhiteSpace(queue) &&
            !string.IsNullOrWhiteSpace(method))
        {
            return (queue, method);
        }

        return null;
    }
}