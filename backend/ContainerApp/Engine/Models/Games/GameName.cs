using System.Text.Json.Serialization;

namespace Engine.Models.Games;

[JsonConverter(typeof(JsonStringEnumConverter))]
public enum GameName
{
    WordOrder,
    TypingPractice,
    SpeakingPractice
}

