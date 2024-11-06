using QueueSharp.Model.DurationDistribution;
using QueueSharp.Model.Routing;
using QueueSharp.Model.ServerSelector;
using System.Collections.Frozen;
using System.Diagnostics;

namespace QueueSharp.Model.Components;

/// <summary>
/// Represents the properties associated with a node, including arrival and service duration distributions,
/// server selection strategy, and optional arrival batch size.
/// </summary>
/// <param name="ArrivalDistributionSelector">Distribution selector for determining arrival times of individuals at this node.</param>
/// <param name="ServiceDurationSelector">Distribution selector for determining service durations at this node.</param>
/// <param name="ServerSelector">Defines the server selection strategy for the node.</param>
/// <param name="ArrivalBatchSize">Optional batch size for arrivals; defaults to 1, indicating the number of individuals arriving at the same time.</param>
public record NodeProperties(DurationDistributionSelector ArrivalDistributionSelector,
    DurationDistributionSelector ServiceDurationSelector,
    IServerSelector ServerSelector,
    int ArrivalBatchSize = 1);

/// <summary>
/// Represents a group of individuals (cohort) with unique properties per node and a routing strategy.
/// Used to organize and configure node interactions within a cohort, including creating individuals with unique IDs.
/// </summary>
/// <param name="Id">The unique identifier for the cohort.</param>
/// <param name="PropertiesByNode">A dictionary mapping nodes to their specific properties within the cohort.</param>
/// <param name="Routing">Defines the routing logic for individuals within the cohort.</param>

[DebuggerDisplay("Cohort '{Id}' with {PropertiesByNode.Count} node properties")]
public record Cohort(string Id,
    FrozenDictionary<Node, NodeProperties> PropertiesByNode,
    IRouting Routing)
{
    private int _id = 0;
    private readonly FrozenDictionary<string, Node> _nodesById = PropertiesByNode
        .ToFrozenDictionary(x => x.Key.Id, x => x.Key);
    internal Individual CreateIndividual()
    {
        if (_id > int.MaxValue - 10)
        {
            ;
        }
        return new Individual()
        {
            Cohort = this,
            Id = ++_id,
        };
    }

    public NodeProperties GetPropertiesById(string nodeId) => PropertiesByNode[_nodesById[nodeId]];
};