namespace Manager.Models.GameConfiguration;

public class UserNewGameConfig
{
    public required GameName GameName { get; set; }
    public required string Difficulty { get; set; }
    public required bool Nikud { get; set; }
    public required int NumberOfSentences { get; set; }
}
