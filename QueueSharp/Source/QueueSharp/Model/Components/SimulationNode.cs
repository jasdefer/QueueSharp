using System.Diagnostics;

namespace QueueSharp.Model.Components;

internal record Overflow(Individual Individual, SimulationNode BlockedNode, int BlockedServer, int ArrivalTime);

[DebuggerDisplay("Node '{Id}' with {ServerCount} servers")]
internal class SimulationNode
{
    /// <summary>
    /// The unique identifier for the Node
    /// </summary>
    internal string Id { get; }

    /// <summary>
    /// The number of servers in this node
    /// </summary>
    internal int ServerCount { get; private set; }

    /// <summary>
    /// An array with the length equal to the <see cref="ServerCount"/>.
    /// Each element is null if the server is idle and set to an individual with the individual being served.
    /// </summary>
    internal Individual?[] ServingIndividuals { get; private set; } = [];

    /// <summary>
    /// Null, if the queue has no capacity.
    /// Otherwise the maximum number of individuals being in the queue.
    /// </summary>
    internal int? QueueCapacity { get; set; }

    /// <summary>
    /// The list of individuals and their arrival time at this node who are waiting in the queue to be served by one of the servers.
    /// </summary>
    internal Queue<(Individual, int)> Queue { get; } = [];

    /// <summary>
    /// Individuals targeting this node after leaving other nodes are prevented from enqueueing if this node's queue is full.
    /// They can reject or block the current server of the origin node and queue up in this <see cref="OverflowQueue"/> until capacity in the destination queue is available again.
    /// </summary>
    internal Queue<Overflow> OverflowQueue { get; } = [];

    internal SimulationNode(string id, int serverCount, int? queueCapacity = null)
    {
        Id = id;
        QueueCapacity = queueCapacity;
        SetServerCount(serverCount);
    }

    internal void SetServerCount(int serverCount)
    {
        ServerCount = serverCount;
        ServingIndividuals = new Individual?[serverCount];
    }

    public override int GetHashCode()
    {
        return Id.GetHashCode();
    }

    internal bool IsQueueFull => Queue.Count >= QueueCapacity;
    internal bool IsQueueEmpty => Queue.Count == 0;
}
