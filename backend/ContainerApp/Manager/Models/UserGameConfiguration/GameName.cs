using System.Runtime.Serialization;
namespace Manager.Models.UserGameConfiguration;

public enum GameName
{
    [EnumMember(Value = "WordOrder")]
    WordOrder,

    [EnumMember(Value = "TypingPractice")]
    TypingPractice,

    [EnumMember(Value = "SpeakingPractice")]
    SpeakingPractice
}

