namespace Accessor.Models;

public record UpdateTaskResult(
    bool Updated,
    bool NotFound,
    bool PreconditionFailed,
    string? NewEtag
);

