using System.Text.Json;
using Engine.Messaging;
using Engine.Models;

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

    public static void ValidateAiRequest(AiRequestModel req, ILogger logger)
    {
        if (req is null)
        {
            logger.LogWarning("AiRequestModel cannot be null.");
            throw new NonRetryableException("AiRequestModel cannot be null.");
        }

        if (string.IsNullOrWhiteSpace(req.Id))
        {
            logger.LogWarning("Request Id is required.");
            throw new NonRetryableException("Request Id is required.");
        }

        if (string.IsNullOrWhiteSpace(req.ThreadId))
        {
            logger.LogWarning("ThreadId is required.");
            throw new NonRetryableException("ThreadId is required.");
        }

        if (string.IsNullOrWhiteSpace(req.Question))
        {
            logger.LogWarning("Question is required.");
            throw new NonRetryableException("Question is required.");
        }


    }
