namespace Accessor.Routing;

public interface IRoutingContextAccessor
{
    RoutingContext? Current { get; set; }
}
