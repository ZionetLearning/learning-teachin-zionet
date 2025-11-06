using System.Text.Json.Serialization;

namespace Engine.Models.Users;

[JsonConverter(typeof(JsonStringEnumConverter))]
public enum SupportedLanguage
{
    en, // English
    he // Hebrew
}
