namespace Manager.Models;
public record UpdateTaskNameResult(
    bool Updated,
    bool NotFound,
    bool PreconditionFailed,
    string? NewEtag
);
