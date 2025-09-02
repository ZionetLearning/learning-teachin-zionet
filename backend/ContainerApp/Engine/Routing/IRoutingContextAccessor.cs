namespace Engine.Routing;

public interface IRoutingContextAccessor
{
    RoutingContext? Current { get; set; }
}
