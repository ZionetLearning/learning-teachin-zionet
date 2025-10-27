using System.Text.Json;
using System.Text.Json.Serialization;

namespace Manager.Models.QueueMessages;

public record Message
{
    [JsonConverter(typeof(JsonStringEnumConverter))]
    public MessageAction ActionName { get; set; }
    public JsonElement Payload { get; set; }
    public JsonElement? Metadata { get; set; } = null;
}
[JsonConverter(typeof(JsonStringEnumConverter))]
public enum MessageAction
{
    CreateTask,
    UpdateTask,
    TestLongTask,
    AnswerAi,
    ProcessingChatMessage,
    ProcessingExplainMistake,
    NotifyUser,
    GenerateSentences,
    GenerateSplitSentences
}
