using QueueSharp.Model.Components;
using System.Diagnostics;

namespace QueueSharp.Model.Events;

[DebuggerDisplay("{Timestamp}: Arrival  at {Node.Id} from {Individual.Id}")]
public record ArrivalEvent(int Timestamp,
    Individual Individual,
    Node Node) : IEvent;
