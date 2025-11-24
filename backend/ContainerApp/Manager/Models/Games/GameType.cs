using System.Text.Json.Serialization;

namespace Manager.Models.Games;

[JsonConverter(typeof(JsonStringEnumConverter))]
public enum GameType
{
    WordOrderGame,
    TypingPractice,
    SpeakingPractice
}
