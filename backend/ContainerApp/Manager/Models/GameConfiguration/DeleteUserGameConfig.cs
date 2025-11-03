namespace Manager.Models.GameConfiguration;

public class DeleteUserGameConfig
{
    public required Guid UserId { get; set; }
    public required GameName GameName { get; set; }
}
