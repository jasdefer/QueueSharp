using QueueSharp.Model.Components;
using QueueSharp.Model.Exceptions;
using System.Collections.Frozen;
using System.Collections.Immutable;

namespace QueueSharp.Model.Routing;

internal class RandomRouteSelection : IRouting
{
    private readonly FrozenDictionary<Node, ImmutableArray<WeightedArc>> _arcsByOrigin;
    private readonly FrozenDictionary<Node, double> _totalWeight;
    private readonly Random _random;
    private readonly QueueIsFullBehavior _queueIsFullBehavior;

    public RandomRouteSelection(IEnumerable<WeightedArc> arcs,
        QueueIsFullBehavior queueIsFullBehavior,
        int? randomSeed)
    {
        _arcsByOrigin = arcs.GroupBy(x => x.Origin)
            .ToFrozenDictionary(x => x.Key,
            x => x.ToImmutableArray());
        _random = randomSeed is null
            ? new Random()
            : new Random(randomSeed.Value);
        _queueIsFullBehavior = queueIsFullBehavior;
        _totalWeight = _arcsByOrigin.ToFrozenDictionary(x => x.Key, x => x.Value.Sum(y => y.Weight));
    }

    public RoutingDecision RouteAfterService(Node origin, State state)
    {
        bool containsNode = _arcsByOrigin.TryGetValue(origin, out ImmutableArray<WeightedArc> outoingArcs);
        if (!containsNode || outoingArcs.Length == 0)
        {
            return ExitSystem.StaticInstance;
        }

        if (outoingArcs.Length == 1)
        {
            Node destination = outoingArcs[0].Destination; // improve performance by skipping the random call
            return new SeekDestination(destination, _queueIsFullBehavior);
        }

        double randomValue = _random.NextDouble() * _totalWeight[origin];
        double cumulativeWeight = 0;
        for (int i = 0; i < outoingArcs.Length; i++)
        {
            cumulativeWeight += outoingArcs[i].Weight;
            if (randomValue <= cumulativeWeight)
            {
                return new SeekDestination(outoingArcs[i].Destination, _queueIsFullBehavior);
            }
        }
        throw new ImplausibleStateException("An arc should have been selected.");
    }
}