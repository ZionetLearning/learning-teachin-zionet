using Manager.Models.Meetings;

namespace Manager.Helpers;

public static class MeetingValidationHelper
{
    public static List<string> ValidateCreateMeetingRequest(CreateMeetingRequest request, MeetingOptions options)
    {
        var errors = new List<string>();

        if (request.Attendees == null || request.Attendees.Count < options.MinAttendees)
        {
            errors.Add($"Meeting must have at least {options.MinAttendees} attendees.");
        }
        else if (request.Attendees.Count > options.MaxAttendees)
        {
            errors.Add($"Meeting cannot have more than {options.MaxAttendees} attendees.");
        }

        if (request.DurationMinutes < options.MinDurationMinutes)
        {
            errors.Add($"Meeting duration must be at least {options.MinDurationMinutes} minute(s).");
        }
        else if (request.DurationMinutes > options.MaxDurationMinutes)
        {
            errors.Add($"Meeting duration cannot exceed {options.MaxDurationMinutes} minutes ({options.MaxDurationMinutes / 60} hours).");
        }

        if (!string.IsNullOrEmpty(request.Description) && request.Description.Length > options.MaxDescriptionLength)
        {
            errors.Add($"Meeting description cannot exceed {options.MaxDescriptionLength} characters.");
        }

        var now = DateTimeOffset.UtcNow;
        var minStartTime = now.AddMinutes(options.MinAdvanceSchedulingMinutes);
        var maxStartTime = now.AddDays(options.MaxAdvanceSchedulingDays);

        if (request.StartTimeUtc < minStartTime)
        {
            errors.Add($"Meeting must be scheduled at least {options.MinAdvanceSchedulingMinutes} minutes in advance.");
        }
        else if (request.StartTimeUtc > maxStartTime)
        {
            errors.Add($"Meeting cannot be scheduled more than {options.MaxAdvanceSchedulingDays} days in advance.");
        }

        if (request.Attendees != null)
        {
            var duplicateUserIds = request.Attendees
                .GroupBy(a => a.UserId)
                .Where(g => g.Count() > 1)
                .Select(g => g.Key)
                .ToList();

            if (duplicateUserIds.Any())
            {
                errors.Add($"Duplicate attendees found: {string.Join(", ", duplicateUserIds)}");
            }
        }

        return errors;
    }

    public static List<string> ValidateUpdateMeetingRequest(UpdateMeetingRequest request, MeetingOptions options)
    {
        var errors = new List<string>();

        if (request.Attendees != null)
        {
            if (request.Attendees.Count < options.MinAttendees)
            {
                errors.Add($"Meeting must have at least {options.MinAttendees} attendees.");
            }
            else if (request.Attendees.Count > options.MaxAttendees)
            {
                errors.Add($"Meeting cannot have more than {options.MaxAttendees} attendees.");
            }

            var duplicateUserIds = request.Attendees
                .GroupBy(a => a.UserId)
                .Where(g => g.Count() > 1)
                .Select(g => g.Key)
                .ToList();

            if (duplicateUserIds.Any())
            {
                errors.Add($"Duplicate attendees found: {string.Join(", ", duplicateUserIds)}");
            }
        }

        if (request.DurationMinutes.HasValue)
        {
            if (request.DurationMinutes.Value < options.MinDurationMinutes)
            {
                errors.Add($"Meeting duration must be at least {options.MinDurationMinutes} minute(s).");
            }
            else if (request.DurationMinutes.Value > options.MaxDurationMinutes)
            {
                errors.Add($"Meeting duration cannot exceed {options.MaxDurationMinutes} minutes ({options.MaxDurationMinutes / 60} hours).");
            }
        }

        if (!string.IsNullOrEmpty(request.Description) && request.Description.Length > options.MaxDescriptionLength)
        {
            errors.Add($"Meeting description cannot exceed {options.MaxDescriptionLength} characters.");
        }

        if (request.StartTimeUtc.HasValue)
        {
            var now = DateTimeOffset.UtcNow;
            var minStartTime = now.AddMinutes(options.MinAdvanceSchedulingMinutes);
            var maxStartTime = now.AddDays(options.MaxAdvanceSchedulingDays);

            if (request.StartTimeUtc.Value < minStartTime)
            {
                errors.Add($"Meeting must be scheduled at least {options.MinAdvanceSchedulingMinutes} minutes in advance.");
            }
            else if (request.StartTimeUtc.Value > maxStartTime)
            {
                errors.Add($"Meeting cannot be scheduled more than {options.MaxAdvanceSchedulingDays} days in advance.");
            }
        }

        return errors;
    }
}
