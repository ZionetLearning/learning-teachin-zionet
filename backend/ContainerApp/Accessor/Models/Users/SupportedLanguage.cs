using System.Text.Json.Serialization;

namespace Accessor.Models.Users;

[JsonConverter(typeof(JsonStringEnumConverter))]
public enum SupportedLanguage
{
    en, // English
    he // Hebrew
}
