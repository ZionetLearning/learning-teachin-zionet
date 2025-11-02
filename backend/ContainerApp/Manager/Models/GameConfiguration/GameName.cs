using System.Runtime.Serialization;
namespace Manager.Models.GameConfiguration;

public enum GameName
{
    [EnumMember(Value = "WordOrder")]
    WordOrder,

    [EnumMember(Value = "TypingPractice")]
    TypingPractice,

    [EnumMember(Value = "SpeakingPractice")]
    SpeakingPractice
}

