using System.Text.Json.Serialization;

namespace Manager.Models.Meetings;

[JsonConverter(typeof(JsonStringEnumConverter))]
public enum AttendeeRole
{
    Teacher,
    Student
}
