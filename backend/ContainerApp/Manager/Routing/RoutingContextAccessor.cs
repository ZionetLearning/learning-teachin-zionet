namespace Manager.Routing;

public class RoutingContextAccessor : IRoutingContextAccessor
{
    private static readonly AsyncLocal<RoutingContext?> _current = new();

    public RoutingContext? Current
    {
        get => _current.Value;
        set => _current.Value = value;
    }
}