using System.Text.Json.Serialization;

namespace Engine.Models.Sentences;

[JsonConverter(typeof(JsonStringEnumConverter))]
public enum GameType
{
    WordOrderGame,
    TypingPractice,
    SpeakingPractice
}
