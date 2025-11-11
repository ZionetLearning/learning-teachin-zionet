using System.ComponentModel;
using System.Text.Json.Serialization;

namespace Engine.Models.Games;

[JsonConverter(typeof(JsonStringEnumConverter))]
public enum GameName
{
    [Description("word order")]
    WordOrder,

    [Description("typing practice")]
    TypingPractice,

    [Description("speaking practice")]
    SpeakingPractice
}

