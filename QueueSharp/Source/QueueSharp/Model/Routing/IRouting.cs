using QueueSharp.Model.Components;

namespace QueueSharp.Model.Routing;

/// <summary>
/// Defines behavior for handling an individual when the destination node's queue is full during routing.
/// </summary>
public enum QueueIsFullBehavior
{
    /// <summary>
    /// The individual will wait at the origin node and block the current server until space becomes available in the destination queue.
    /// This behavior may impact the server’s availability for new individuals at the origin node.
    /// </summary>
    WaitAndBlockCurrentServer,
    /// <summary>
    /// The individual is rejected from the system if its destination queue is full.
    /// This allows the server at the origin node to remain available for other individuals.
    /// </summary>
    RejectIndividual
}

/// <summary>
/// Defines the interface for routing decisions after service completion at a node.
/// Implementations specify how an individual is routed from the origin node to the next destination.
/// </summary>
public interface IRouting
{ 
    /// <summary>
    /// Determines the routing decision after completing service at the origin node.
    /// </summary>
    /// <param name="origin">The node where service was just completed.</param>
    /// <param name="systemNodes">A collection of all nodes in the system, which can influence the routing decisions.</param>
    /// <returns>A <see cref="RoutingDecision"/> that specifies the next action for the individual.</returns>
    RoutingDecision RouteAfterService(Node origin, IEnumerable<Node> systemNodes);
}

/// <summary>
/// Represents a base class for routing decisions after service completion. 
/// Used to define the next action for an individual, such as exiting the system or seeking a new destination.
/// </summary>
public abstract record RoutingDecision;

/// <summary>
/// Represents a routing decision where the individual exits the system after completing service.
/// </summary>
public sealed record ExitSystem : RoutingDecision
{
    internal static ExitSystem StaticInstance { get; } = new ExitSystem();
};

/// <summary>
/// Represents a routing decision where the individual seeks a specified destination node after service.
/// Includes behavior control if the destination node's queue is full.
/// </summary>
/// <param name="Destination">The target node to which the individual should be routed.</param>
/// <param name="QueueIsFullBehavior">Specifies the action to take if the destination node's queue is full.</param>

public sealed record SeekDestination(Node Destination, QueueIsFullBehavior QueueIsFullBehavior) : RoutingDecision;