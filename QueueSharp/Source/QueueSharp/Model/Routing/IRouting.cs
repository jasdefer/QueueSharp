using QueueSharp.Model.Components;

namespace QueueSharp.Model.Routing;
internal interface IRouting
{
    bool TryGetDestination(Node origin, State state, out Node? destination);
}
