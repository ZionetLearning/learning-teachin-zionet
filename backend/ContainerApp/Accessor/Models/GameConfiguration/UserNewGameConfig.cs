using Accessor.Models.Games;

namespace Accessor.Models.GameConfiguration;

public class UserNewGameConfig
{
    public required GameName GameName { get; set; }
    public required Difficulty Difficulty { get; set; }
    public required bool Nikud { get; set; }
    public required int NumberOfSentences { get; set; }
}
