using QueueSharp.Model.Components;

namespace QueueSharp.Model.Events;
internal record ArrivalEvent(int Timestamp,
    Individual Individual,
    Node Node) : IEvent;
