using QueueSharp.Model.DurationDistribution;
using QueueSharp.Model.Routing;
using QueueSharp.Model.ServerSelector;
using System.Collections.Frozen;
using System.Collections.Immutable;

namespace QueueSharp.Model.Components;

internal record NodeProperties(DurationDistributionSelector DurationDistributionSelector,
    DurationDistributionSelector ServiceDurationSelector,
    IServerSelector ServerSelector,
    int ArrivalBatchSize = 1);

internal record Cohort(FrozenDictionary<Node, NodeProperties> PropertiesByNode,
    ImmutableArray<Arc> Arcs,
    IRouting Routing,
    int ArrivalBatchSize = 1)
{
    private int _id = 0;
    internal Individual CreateIndividual()
    {
        return new Individual()
        {
            Cohort = this,
            Id = ++_id,
        };
    }
};