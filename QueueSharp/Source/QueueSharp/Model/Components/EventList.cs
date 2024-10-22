using QueueSharp.Model.Events;

namespace QueueSharp.Model.Components;
public class EventList
{
    /// <summary>
    /// The list of events
    /// Is always sorted by the timestamp
    /// </summary>
    private readonly List<IEvent> _events = [];

    internal IEvent Dequeue()
    {
        IEvent @event = _events[0];
        _events.RemoveAt(0);
        return @event;
    }

    internal void Insert(IEvent newEvent)
    {
        // ToDo: Use heap to improve performance
        for (int i = 0; i < _events.Count; i++)
        {
            if (newEvent.Timestamp < _events[i].Timestamp)
            {
                _events.Insert(i, newEvent);
                return;
            }
        }
        _events.Add(newEvent);
    }

    public bool IsEmpty => _events.Count == 0;
}
