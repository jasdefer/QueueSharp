using QueueSharp.Model.Components;
using QueueSharp.Model.Exceptions;
using System.Collections.Frozen;
using System.Collections.Immutable;

namespace QueueSharp.Model.Routing;

internal class RandomRouteSelection : IRouting
{
    private readonly FrozenDictionary<Node, ImmutableArray<Arc>> _arcsByOrigin;
    private readonly Random _random;

    public RandomRouteSelection(IEnumerable<Arc> arcs,
        int? randomSeed)
    {
        _arcsByOrigin = arcs.GroupBy(x => x.Origin)
            .ToFrozenDictionary(x => x.Key,
            x => x.ToImmutableArray());
        _random = randomSeed is null
            ? new Random()
            : new Random(randomSeed.Value);
    }
    public bool TryGetDestination(Node node, State state, out Node? destination)
    {
        bool containsNode = _arcsByOrigin.TryGetValue(node, out ImmutableArray<Arc> outoingArcs);
        if (!containsNode || outoingArcs.Length == 0)
        {
            destination = null;
            return false;
        }
        destination =  outoingArcs[_random.Next(0, outoingArcs.Length)].Destination;
        return true;
    }
}