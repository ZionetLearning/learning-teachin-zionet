namespace Engine.Constants
{
    public static class Prompts
    {
        public static readonly string SystemDefault =
            "You are a helpful assistant. Maintain context. Keep your answers brief, clear and helpful.";

        // other blanks that can be combined later
        public static readonly string FriendlyTone =
            "Speak in a friendly manner, as if you were speaking to a colleague.";

        public static readonly string DetailedExplanation =
            "Let's go into detail, step by step, so that even a beginner can understand.";


        public static string Combine(params string[] parts)
        {
            return string.Join(" ", parts.Where(p => !string.IsNullOrWhiteSpace(p)));
        }
    }
}