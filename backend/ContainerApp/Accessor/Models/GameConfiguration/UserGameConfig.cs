namespace Accessor.Models.GameConfiguration;

public class UserGameConfig
{
    public required Guid UserId { get; set; }
    public required GameName GameName { get; set; }
    public required string Difficulty { get; set; }
    public required bool Nikud { get; set; }
    public required int NumberOfSentences { get; set; }
}
