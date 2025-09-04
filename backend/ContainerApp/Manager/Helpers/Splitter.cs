using Manager.Models.Sentences;

namespace Manager.Helpers;

public static class Splitter
{
    public static SplitSentenceResponse Split(SentenceResponse? input)
    {
        var result = new SplitSentenceResponse();

        if (input?.Sentences is null || input.Sentences.Count == 0)
        {
            return result;
        }

        foreach (var s in input.Sentences)
        {
            var words = s?.Text?
                .Split((char[]?)null, StringSplitOptions.RemoveEmptyEntries)
                .ToList() ?? new List<string>();

            result.Sentences.Add(new SplitSentenceItem
            {
                Words = words,
                Original = s?.Text ?? string.Empty,
                Difficulty = s?.Difficulty ?? string.Empty,
                Nikud = s?.Nikud ?? false
            });
        }

        return result;
    }

    public static SplitSentenceItem SplitOne(SentenceItem s)
    {
        var words = s?.Text?
            .Split((char[]?)null, StringSplitOptions.RemoveEmptyEntries)
            .ToList() ?? new List<string>();

        return new SplitSentenceItem
        {
            Words = words,
            Original = s?.Text ?? string.Empty,
            Difficulty = s?.Difficulty ?? string.Empty,
            Nikud = s?.Nikud ?? false
        };
    }
}
