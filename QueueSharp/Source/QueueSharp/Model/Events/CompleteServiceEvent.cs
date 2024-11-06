using QueueSharp.Model.Components;
using System.Diagnostics;

namespace QueueSharp.Model.Events;

[DebuggerDisplay("{Timestamp}: Completed {Individual.Id} at {Node.Id}")]
internal record CompleteServiceEvent(int Timestamp,
    int Server,
    Individual Individual,
    SimulationNode Node,
    int ArrivalTime) : IEvent;