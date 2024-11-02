using QueueSharp.Model.Components;

namespace QueueSharp.Model.Routing;
public enum QueueIsFullBehavior
{
    WaitAndBlockCurrentServer,
    RejectIndividual
}

public interface IRouting
{
    RoutingDecision RouteAfterService(Node origin, State state);
}
public abstract record RoutingDecision;
public sealed record ExitSystem : RoutingDecision
{
    internal static ExitSystem StaticInstance { get; } = new ExitSystem();
};
public sealed record SeekDestination(Node Destination, QueueIsFullBehavior QueueIsFullBehavior) : RoutingDecision;