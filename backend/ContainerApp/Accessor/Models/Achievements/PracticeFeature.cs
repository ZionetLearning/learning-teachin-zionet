using System.Text.Json.Serialization;

namespace Accessor.Models.Achievements;

[JsonConverter(typeof(JsonStringEnumConverter))]
public enum PracticeFeature
{
    WordCards,
    PracticeMistakes,
    WordOrder,
    TypingPractice,
    SpeakingPractice,
    WordCardsChallenge
}
