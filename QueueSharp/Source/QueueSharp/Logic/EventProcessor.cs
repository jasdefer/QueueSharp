using QueueSharp.Model.Components;
using QueueSharp.Model.Events;
using QueueSharp.Model.Exceptions;

namespace QueueSharp.Logic;
internal class EventProcessor
{
    private readonly State _state;

    public EventProcessor(State state)
    {
        _state = state;
    }

    internal void ProcessArrival(ArrivalEvent arrivalEvent)
    {
        throw new NotImplementedException();
    }

    internal void ProcessEvent(IEvent @event)
    {
        switch (@event) {
            case ArrivalEvent arrivalEvent:
                ProcessArrival(arrivalEvent);
                return;
            default:
                throw new NotImplementedEventException(@event.GetType().Name);
        }
    }
}
