namespace QueueSharp.Model.Components;

/// <summary>
/// A node is a collection of servers serving individuals in the simulation.
/// </summary>
/// <param name="Id">A unique id of the node.</param>
/// <param name="ServerCount">The number of servers at this node.</param>
/// <param name="QueueCapacity">The maximum number of individuals that can queue at this node.</param>
public record Node(string Id,
    int ServerCount,
    int? QueueCapacity = null);
