using QueueSharp.Model.Components;

namespace QueueSharp.Model.Routing;
public enum QueueIsFullBehavior
{
    WaitAndBlockCurrentServer,
    Baulk
}

internal interface IRouting
{
    RoutingDecision RouteAfterService(Node origin, State state);
}
internal abstract record RoutingDecision;
internal sealed record ExitSystem : RoutingDecision
{
    internal static ExitSystem StaticInstance { get; } = new ExitSystem();
};
internal sealed record SeekDestination(Node Destination, QueueIsFullBehavior QueueIsFullBehavior) : RoutingDecision;