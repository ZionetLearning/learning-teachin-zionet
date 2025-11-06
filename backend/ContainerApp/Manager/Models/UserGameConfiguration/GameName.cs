using System.Text.Json.Serialization;

namespace Manager.Models.UserGameConfiguration;

[JsonConverter(typeof(JsonStringEnumConverter))]
public enum GameName
{
    WordOrder,
    TypingPractice,
    SpeakingPractice
}

