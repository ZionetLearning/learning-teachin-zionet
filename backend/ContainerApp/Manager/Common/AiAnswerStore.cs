using System.Collections.Concurrent;

namespace Manager.Common;

public static class AiAnswerStore
{
    public static readonly ConcurrentDictionary<string, string> Answers = new();
}

