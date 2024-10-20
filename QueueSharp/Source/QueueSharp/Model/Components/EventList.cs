using QueueSharp.Model.Events;

namespace QueueSharp.Model.Components;
internal class EventList
{
    private readonly List<IEvent> _events = [];

    public IEvent Dequeue()
    {
        IEvent @event = _events[0];
        _events.RemoveAt(0);
        return @event;
    }

    public bool IsEmpty => _events.Count == 0;
}
