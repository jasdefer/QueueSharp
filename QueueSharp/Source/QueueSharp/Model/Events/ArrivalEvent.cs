using QueueSharp.Model.Components;

namespace QueueSharp.Model.Events;
public record ArrivalEvent(int Timestamp,
    Individual Individual,
    Node Node) : IEvent;
