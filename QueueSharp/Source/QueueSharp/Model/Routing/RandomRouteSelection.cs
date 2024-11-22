using QueueSharp.Model.Components;
using QueueSharp.Model.Exceptions;
using System.Collections.Frozen;
using System.Collections.Immutable;

namespace QueueSharp.Model.Routing;

/// <summary>
/// Randomly select a destination considering weights.
/// </summary>
public class RandomRouteSelection : IRouting
{
    private readonly FrozenDictionary<Node, ImmutableArray<WeightedArc>> _arcsByOrigin;
    private readonly FrozenDictionary<Node, double> _totalWeight;
    private readonly FrozenDictionary<Node, QueueIsFullBehavior> _queueIsFullBehavior;

    /// <summary>
    /// Create a new instance. 
    /// </summary>
    /// <param name="arcs">Arcs describing possible destinations and the corresponding weight used for the random selection.</param>
    /// <param name="queueIsFullBehavior">Indicates, the behavior of the individual when it arrives at a node with a full queue.</param>
    public RandomRouteSelection(IEnumerable<WeightedArc> arcs,
        FrozenDictionary<Node, QueueIsFullBehavior>? queueIsFullBehavior)
    {
        _arcsByOrigin = arcs.GroupBy(x => x.Origin)
            .ToFrozenDictionary(x => x.Key,
            x => x.ToImmutableArray());
        _queueIsFullBehavior = queueIsFullBehavior ?? arcs
            .Select(x => x.Origin)
            .Union(arcs.Select(x => x.Destination))
            .Distinct()
            .ToFrozenDictionary(x => x, x => QueueIsFullBehavior.RejectIndividual);
        _totalWeight = _arcsByOrigin.ToFrozenDictionary(x => x.Key, x => x.Value.Sum(y => y.Weight));
    }

    /// <summary>
    /// Returns the <see cref="RoutingDecision"/> when an individual leaves the <paramref name="origin"/> Node.
    /// </summary>
    public RoutingDecision RouteAfterService(Node origin, IEnumerable<Node> systemNodes, Random? random)
    {
        bool containsNode = _arcsByOrigin.TryGetValue(origin, out ImmutableArray<WeightedArc> outoingArcs);
        if (!containsNode || outoingArcs.Length == 0)
        {
            return ExitSystem.StaticInstance;
        }

        if (outoingArcs.Length == 1)
        {
            Node destination = outoingArcs[0].Destination; // improve performance by skipping the random call
            return new SeekDestination(destination, _queueIsFullBehavior[destination]);
        }

        random ??= new Random();
        double randomValue = random.NextDouble() * _totalWeight[origin];
        double cumulativeWeight = 0;
        for (int i = 0; i < outoingArcs.Length; i++)
        {
            cumulativeWeight += outoingArcs[i].Weight;
            if (randomValue <= cumulativeWeight)
            {
                Node destination = outoingArcs[i].Destination;
                return new SeekDestination(destination, _queueIsFullBehavior[destination]);
            }
        }
        throw new ImplausibleStateException("An arc should have been selected.");
    }
}