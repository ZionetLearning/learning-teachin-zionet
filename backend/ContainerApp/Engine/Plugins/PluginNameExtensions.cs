namespace Engine.Plugins;

public static class PluginNameExtensions
{
    private const string Suffix = "Plugin";

    public static string ToPluginName(this Type type)
    {
        var name = type.Name;
        return name.EndsWith(Suffix, StringComparison.Ordinal)
            ? name[..^Suffix.Length]
            : name;
    }
}
