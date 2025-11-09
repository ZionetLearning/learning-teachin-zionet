using System.Text.Json.Serialization;

namespace Accessor.Models.Meetings;

[JsonConverter(typeof(JsonStringEnumConverter))]
public enum AttendeeRole
{
    Teacher,
    Student
}
