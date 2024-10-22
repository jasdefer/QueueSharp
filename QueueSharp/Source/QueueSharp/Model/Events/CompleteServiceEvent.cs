using QueueSharp.Model.Components;

namespace QueueSharp.Model.Events;
public record CompleteServiceEvent(int Timestamp,
    int Server,
    Individual Individual,
    Node Node,
    int ArrivalTime) : IEvent;