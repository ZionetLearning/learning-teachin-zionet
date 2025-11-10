namespace Manager.Models.Meetings;

public sealed class MeetingOptions
{
    public const string SectionName = "Meetings";

    public int MaxDurationMinutes { get; set; } = 480;

    public int MinDurationMinutes { get; set; } = 1;

    public int MaxAttendees { get; set; } = 50;

    public int MinAttendees { get; set; } = 2;

    public int MaxAdvanceSchedulingDays { get; set; } = 90;

    public int MinAdvanceSchedulingMinutes { get; set; } = 5;

    public int MaxDescriptionLength { get; set; } = 500;
}
