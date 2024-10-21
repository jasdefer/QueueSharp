using QueueSharp.Model.DurationDistribution;
using QueueSharp.Model.Routing;
using QueueSharp.Model.ServerSelector;
using System.Collections.Frozen;

namespace QueueSharp.Model.Components;

internal record NodeProperties(DurationDistributionSelector DurationDistributionSelector,
    DurationDistributionSelector ServiceDurationSelector,
    IServerSelector ServerSelector,
    int ArrivalBatchSize = 1);

internal record Cohort(string Id,
    FrozenDictionary<Node, NodeProperties> PropertiesByNode,
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