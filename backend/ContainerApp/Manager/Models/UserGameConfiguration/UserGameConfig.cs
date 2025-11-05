using Manager.Models.Games;

namespace Manager.Models.UserGameConfiguration;

public class UserGameConfig
{
    public required Guid UserId { get; set; }
    public required GameName GameName { get; set; }
    public required Difficulty Difficulty { get; set; }
    public required bool Nikud { get; set; }
    public required int NumberOfSentences { get; set; }
}
