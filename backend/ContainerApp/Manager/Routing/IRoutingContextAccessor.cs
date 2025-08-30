namespace Manager.Routing;

public interface IRoutingContextAccessor
{
    RoutingContext? Current { get; set; }
}
