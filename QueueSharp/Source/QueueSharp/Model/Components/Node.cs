using System.Diagnostics;

namespace QueueSharp.Model.Components;

[DebuggerDisplay("Node '{Id}' with {ServerCount} servers")]
public class Node
{
    /// <summary>
    /// The unique identifier for the Node
    /// </summary>
    public string Id { get; }

    /// <summary>
    /// The number of servers in this node
    /// </summary>
    public int ServerCount { get; private set; }

    /// <summary>
    /// An array with the length equal to the <see cref="ServerCount"/>.
    /// Each element is null if the server is idle and set to an individual with the individual being served.
    /// </summary>
    public Individual?[] ServingIndividuals { get; private set; } = [];

    /// <summary>
    /// Null, if the queue has no capacity.
    /// Otherwise the maximum number of individuals being in the queue.
    /// </summary>
    public int? QueueCapacity { get; set; }

    /// <summary>
    /// The list of individuals and their arrival time at this node who are waiting in the queue to be served by one of the servers.
    /// </summary>
    public List<(Individual, int)> Queue { get; } = [];

    /// <summary>
    /// Individuals targeting this node after leaving other nodes are prevented from enqueueing if this node's queue is full.
    /// They can baulk or block the current server of the origin node and queue up in this <see cref="OverflowQueue"/> until capacity in the destination queue is available again.
    /// </summary>
    public List<Individual> OverflowQueue { get; } = [];

    public Node(string id, int serverCount)
    {
        Id = id;
        SetServerCount(serverCount);
    }

    public void SetServerCount(int serverCount)
    {
        ServerCount = serverCount;
        ServingIndividuals = new Individual?[serverCount];
    }

    public override int GetHashCode()
    {
        return Id.GetHashCode();
    }

    public bool IsQueueFull => Queue.Count >= QueueCapacity;
    public bool IsQueueEmpty => Queue.Count == 0;
}
