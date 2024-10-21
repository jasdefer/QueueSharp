using QueueSharp.Model.Components;
using QueueSharp.Model.Events;
using QueueSharp.Model.Exceptions;

namespace QueueSharp.Logic;
internal class EventProcessor
{
    private readonly State _state;

    internal EventProcessor(State state)
    {
        _state = state;
    }

    internal void ProcessEvents()
    {
        while (!_state.EventList.IsEmpty)
        {
            IEvent currentEvent = _state.EventList.Dequeue();
            ProcessEvent(currentEvent);
        }
    }

    private void ProcessArrival(ArrivalEvent arrivalEvent)
    {
        throw new NotImplementedException();
    }

    private void ProcessEvent(IEvent @event)
    {
        switch (@event)
        {
            case ArrivalEvent arrivalEvent:
                ProcessArrival(arrivalEvent);
                return;
            default:
                throw new NotImplementedEventException(@event.GetType().Name);
        }
    }
}
