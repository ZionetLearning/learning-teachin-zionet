namespace Accessor.Helpers;

/// <summary>
/// Provides accuracy calculation methods for different game types.
/// </summary>
public static class AccuracyCalculator
{
    /// <summary>
    /// Calculates accuracy for Word Order Game based on correct word positions.
    /// </summary>
    public static decimal CalculateWordOrderAccuracy(List<string> correctAnswer, List<string> givenAnswer)
    {
        if (correctAnswer.Count == 0)
        {
            return 0m;
        }

        var maxLength = Math.Max(correctAnswer.Count, givenAnswer.Count);
        var correctPositions = 0;

        for (var i = 0; i < maxLength; i++)
        {
            if (i < correctAnswer.Count &&
                i < givenAnswer.Count &&
                correctAnswer[i] == givenAnswer[i])
            {
                correctPositions++;
            }
        }

        return Math.Round((decimal)correctPositions / correctAnswer.Count * 100, 2);
    }

    /// <summary>
    /// Calculates accuracy for Typing Practice and Speaking Practice using Levenshtein distance algorithm.
    /// Returns character-level accuracy percentage.
    /// </summary>
    public static decimal CalculateTextAccuracy(string correctText, string givenText)
    {
        if (string.IsNullOrEmpty(correctText))
        {
            return 0m;
        }

        if (correctText == givenText)
        {
            return 100m;
        }

        var distance = LevenshteinDistance(correctText, givenText);
        var maxLength = Math.Max(correctText.Length, givenText.Length);
        var accuracy = (1 - (decimal)distance / maxLength) * 100;

        return Math.Max(0, Math.Round(accuracy, 2));
    }

    /// <summary>
    /// Calculates accuracy based on game type.
    /// </summary>
    public static decimal Calculate(string gameType, List<string> correctAnswer, List<string> givenAnswer)
    {

        var normalizedGameType = gameType.ToLowerInvariant();
        if (normalizedGameType is "typingPractice" or "speakingPractice")
        {
            var correctText = correctAnswer.FirstOrDefault() ?? string.Empty;
            var givenText = givenAnswer.FirstOrDefault() ?? string.Empty;
            return CalculateTextAccuracy(correctText, givenText);
        }

        return CalculateWordOrderAccuracy(correctAnswer, givenAnswer);
    }

    /// <summary>
    /// Computes Levenshtein distance between two strings.
    /// This measures the minimum number of single-character edits required to change one string into another.
    /// </summary>
    private static int LevenshteinDistance(string source, string target)
    {
        if (string.IsNullOrEmpty(source))
        {
            return target?.Length ?? 0;
        }

        if (string.IsNullOrEmpty(target))
        {
            return source.Length;
        }

        var sourceLength = source.Length;
        var targetLength = target.Length;
        var distance = new int[sourceLength + 1][];
        for (var i = 0; i <= sourceLength; i++)
        {
            distance[i] = new int[targetLength + 1];
        }

        for (var i = 0; i <= sourceLength; i++)
        {
            distance[i][0] = i;
        }

        for (var j = 0; j <= targetLength; j++)
        {
            distance[0][j] = j;
        }

        for (var i = 1; i <= sourceLength; i++)
        {
            for (var j = 1; j <= targetLength; j++)
            {
                var cost = target[j - 1] == source[i - 1] ? 0 : 1;
                distance[i][j] = Math.Min(
                    Math.Min(distance[i - 1][j] + 1, distance[i][j - 1] + 1),
                    distance[i - 1][j - 1] + cost);
            }
        }

        return distance[sourceLength][targetLength];
    }
}
