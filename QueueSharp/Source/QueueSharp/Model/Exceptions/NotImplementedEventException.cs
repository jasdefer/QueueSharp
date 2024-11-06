namespace QueueSharp.Model.Exceptions;
internal class NotImplementedEventException : QueueSharpException
{
    public NotImplementedEventException(string eventType)
    {
        EventType = eventType;
    }

    public string EventType { get; }
}
