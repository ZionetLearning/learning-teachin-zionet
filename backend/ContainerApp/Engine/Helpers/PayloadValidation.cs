using System.Text.Json;
using Engine.Models;
using Engine.Models.Chat;
using DotQueue;
using Engine.Models.QueueMessages;
using Engine.Models.Sentences;

namespace Engine.Helpers;
public static class PayloadValidation
{
    public static T DeserializeOrThrow<T>(Message msg, ILogger logger)
    {
        try
        {
            var value = msg.Payload.Deserialize<T>();
            if (value is null)
            {
                logger.LogError("Payload deserialization returned null");
                throw new NonRetryableException("Payload deserialization returned null");
            }

            return value;
        }
        catch (JsonException ex)
        {
            logger.LogError(ex, "Invalid JSON payload for {Model}", typeof(T).Name);
            throw new NonRetryableException("Invalid JSON payload.", ex);
        }
    }

    public static void ValidateTask(TaskModel task, ILogger logger)
    {
        if (task.Id <= 0)
        {
            logger.LogWarning("Task Id must be a positive integer. Actual: {Id}", task.Id);
            throw new NonRetryableException("Task Id must be a positive integer.");
        }

        if (string.IsNullOrWhiteSpace(task.Payload))
        {
            logger.LogWarning("Task Payload is required.");
            throw new NonRetryableException("Task Payload is required.");
        }

        if (string.IsNullOrWhiteSpace(task.Name))
        {
            logger.LogWarning("Task Name is required.");
            throw new NonRetryableException("Task Name is required.");
        }
    }

    public static void ValidateEngineChatRequest(EngineChatRequest req, ILogger logger)
    {
        if (req is null)
        {
            logger.LogWarning("EngineChatRequest cannot be null.");
            throw new NonRetryableException("EngineChatRequest cannot be null.");
        }

        using var _ = logger.BeginScope(new
        {
            req.RequestId,
        });

        static void Fail(ILogger log, string message, string param = "")
        {
            if (!string.IsNullOrEmpty(param))
            {
                log.LogWarning("Validation failed for {Param}: {Message}", param, message);
            }
            else
            {
                log.LogWarning("{Message}", message);

            }

            throw new NonRetryableException(message);
        }

        if (string.IsNullOrWhiteSpace(req.RequestId))
        {
            Fail(logger, "RequestId is required.", nameof(req.RequestId));

        }

        if (req.UserId == Guid.Empty)
        {
            Fail(logger, "UserId is required.", nameof(req.UserId));

        }

        if (req.ThreadId == Guid.Empty)
        {
            Fail(logger, "ThreadId is required.", nameof(req.ThreadId));

        }

        if (string.IsNullOrWhiteSpace(req.UserMessage))
        {
            Fail(logger, "UserMessage is required.", nameof(req.UserMessage));

        }

        if (req.TtlSeconds <= 0)
        {
            Fail(logger, "TtlSeconds must be greater than 0.", nameof(req.TtlSeconds));

        }

        if (req.SentAt <= 0)
        {
            Fail(logger, "SentAt must be a valid Unix timestamp.", nameof(req.SentAt));
        }

        var now = DateTimeOffset.UtcNow.ToUnixTimeSeconds();
        if (now > req.SentAt + req.TtlSeconds)
        {
            Fail(logger, "Request TTL expired.", "TTL");
        }

        logger.LogDebug("ChatAiServiseRequest validation passed.");
    }
    public static void ValidateSentenceGenerationRequest(SentenceRequest req, ILogger logger)
    {
        if (req is null)
        {
            logger.LogWarning("EngineChatRequest cannot be null.");
            throw new NonRetryableException("EngineChatRequest cannot be null.");
        }
    }
}
