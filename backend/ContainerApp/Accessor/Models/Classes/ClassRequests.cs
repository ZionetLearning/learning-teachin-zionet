namespace Accessor.Models.Classes;

public record AddMembersRequest(IEnumerable<Guid> UserIds, Guid AddedBy);
public record RemoveMembersRequest(IEnumerable<Guid> UserIds);
