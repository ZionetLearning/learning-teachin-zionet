namespace Accessor.Models.Achievements;

public record UpdateProgressRequest(
    PracticeFeature Feature,
    int Count
);
