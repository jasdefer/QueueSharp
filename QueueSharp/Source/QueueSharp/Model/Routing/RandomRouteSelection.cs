using QueueSharp.Model.Components;
using System.Collections.Frozen;
using System.Collections.Immutable;

namespace QueueSharp.Model.Routing;

internal class RandomRouteSelection : IRouting
{
    private readonly FrozenDictionary<Node, ImmutableArray<Arc>> _arcsByOrigin;
    private readonly Random _random;
    private readonly QueueIsFullBehavior _queueIsFullBehavior;

    public RandomRouteSelection(IEnumerable<Arc> arcs,
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
    }

    public RoutingDecision RouteAfterService(Node origin, State state)
    {
        bool containsNode = _arcsByOrigin.TryGetValue(origin, out ImmutableArray<Arc> outoingArcs);
        if (!containsNode || outoingArcs.Length == 0)
        {
            return ExitSystem.StaticInstance;
        }

        Node destination;
        if (outoingArcs.Length == 1)
        {
            destination = outoingArcs[0].Destination; // improve performance by skipping the random call
        }
        else
        {
            destination = outoingArcs[_random.Next(0, outoingArcs.Length)].Destination;
        }

        return new SeekDestination(destination, _queueIsFullBehavior);
    }
}