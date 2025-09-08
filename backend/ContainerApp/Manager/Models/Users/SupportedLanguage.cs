using System.Text.Json.Serialization;

namespace Manager.Models.Users;

[JsonConverter(typeof(JsonStringEnumConverter))]
public enum SupportedLanguage
{
    en, // English
    he // Hebrew
}
