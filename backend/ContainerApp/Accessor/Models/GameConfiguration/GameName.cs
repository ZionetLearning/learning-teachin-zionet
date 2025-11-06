using System.Text.Json.Serialization;
namespace Accessor.Models.GameConfiguration;

[JsonConverter(typeof(JsonStringEnumConverter))]
public enum GameName
{
    WordOrder,
    TypingPractice,
    SpeakingPractice
}