using System.Runtime.Serialization;
namespace Accessor.Models.GameConfiguration;

public enum GameName
{
    [EnumMember(Value = "WordOrder")]
    WordOrder,

    [EnumMember(Value = "TypingPractice")]
    TypingPractice,

    [EnumMember(Value = "SpeakingPractice")]
    SpeakingPractice
}

