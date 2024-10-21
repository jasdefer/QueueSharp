using System.Collections.Immutable;

namespace QueueSharp.Model.Components;
internal class State
{
    public required EventList EventList { get; init; }
    public ImmutableArray<Node> Node { get; init; }
    public List<ActivityLog> ActivityLogs { get; } = [];
}
