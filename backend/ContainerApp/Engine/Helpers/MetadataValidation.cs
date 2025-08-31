using System.Text.Json;
using Engine.Models.QueueMessages;
using DotQueue;

namespace Engine.Helpers;

public static class MetadataValidation
{
    public static T DeserializeOrThrow<T>(Message message, ILogger logger) where T : class
    {
        if (!message.Metadata.HasValue)
        {
            logger.LogWarning("Missing metadata for action payload.");
            throw new NonRetryableException("Metadata is required but missing.");
        }

        try
        {
            var meta = JsonSerializer.Deserialize<T>(message.Metadata.Value);
            if (meta is null)
            {
                throw new NonRetryableException("Metadata deserialized to null.");
            }

            return meta;
        }
        catch (NonRetryableException)
        {
            throw;
        }
        catch (JsonException ex)
        {
            logger.LogWarning(ex, "Invalid metadata JSON.");
            throw new NonRetryableException("Invalid metadata JSON.", ex);
        }
    }

    public static void ValidateUserContext(UserContextMetadata metadata, ILogger logger)
    {
        if (string.IsNullOrWhiteSpace(metadata.UserId))
        {
            logger.LogWarning("Metadata validation failed: UserId missing.");
            throw new NonRetryableException("Metadata.UserId is required.");
        }

        // Future: validate MessageId format, correlation IDs, schemaVersion, etc.
    }
}