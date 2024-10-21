using QueueSharp.Model.Components;

namespace QueueSharp.Model.Events;
internal record CompleteServiceEvent(int Timestamp,
    int ServiceStart,
    int Server,
    Individual Individual,
    Node Node) : IEvent;