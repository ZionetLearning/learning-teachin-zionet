using System.Text.Json;
using System.Text.Json.Serialization;

namespace Manager.Models.QueueMessages;

public record Message
{
    [JsonConverter(typeof(JsonStringEnumConverter))]
    public MessageAction ActionName { get; set; }
    public JsonElement Payload { get; set; }
}
[JsonConverter(typeof(JsonStringEnumConverter))]
public enum MessageAction
{
    CreateTask,
    UpdateTask,
    TestLongTask,
    ProcessingQuestionAi,
    AnswerAi,
    NotifyUser
}
