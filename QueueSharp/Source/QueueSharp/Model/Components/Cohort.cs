using QueueSharp.Model.DurationDistribution;
using QueueSharp.Model.Routing;
using QueueSharp.Model.ServerSelector;
using System.Collections.Frozen;
using System.Diagnostics;

namespace QueueSharp.Model.Components;

public record NodeProperties(DurationDistributionSelector DurationDistributionSelector,
    DurationDistributionSelector ServiceDurationSelector,
    IServerSelector ServerSelector,
    int ArrivalBatchSize = 1);

[DebuggerDisplay("Cohort '{Id}' with {PropertiesByNode.Count} node properties")]
public record Cohort(string Id,
    FrozenDictionary<Node, NodeProperties> PropertiesByNode,
    IRouting Routing)
{
    private int _id = 0;
    internal Individual CreateIndividual()
    {
        if(_id > int.MaxValue - 10)
        {
            ;
        }
        return new Individual()
        {
            Cohort = this,
            Id = ++_id,
        };
    }
};