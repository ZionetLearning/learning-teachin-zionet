namespace Manager.Models.UserGameConfiguration;

public class UserGameConfigKey
{
    public required Guid UserId { get; set; }
    public required GameName GameName { get; set; }
}
