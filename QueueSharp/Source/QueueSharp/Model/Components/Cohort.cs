using QueueSharp.Model.DurationDistribution;
using QueueSharp.Model.Routing;
using QueueSharp.Model.ServerSelector;
using System.Collections.Frozen;

namespace QueueSharp.Model.Components;

public record NodeProperties(DurationDistributionSelector DurationDistributionSelector,
    DurationDistributionSelector ServiceDurationSelector,
    IServerSelector ServerSelector,
    int ArrivalBatchSize = 1);

public record Cohort(string Id,
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