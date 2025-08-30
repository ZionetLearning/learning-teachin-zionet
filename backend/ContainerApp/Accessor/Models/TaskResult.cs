using System.Text.Json.Serialization;

namespace Accessor.Models;

public record TaskResult(int Id, TaskResultStatus Status);

[JsonConverter(typeof(JsonStringEnumConverter))]
public enum TaskResultStatus
{
    Created,
    Updated,
    Failed
}