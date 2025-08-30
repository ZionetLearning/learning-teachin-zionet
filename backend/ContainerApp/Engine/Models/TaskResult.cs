using System.Text.Json.Serialization;

namespace Engine.Models;

public record TaskResult(int Id, TaskResultStatus Status);

[JsonConverter(typeof(JsonStringEnumConverter))]
public enum TaskResultStatus
{
    Created,
    Updated,
    Failed
}