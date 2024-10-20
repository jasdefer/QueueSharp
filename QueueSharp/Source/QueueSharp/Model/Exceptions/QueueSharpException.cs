namespace QueueSharp.Model.Exceptions;
internal class QueueSharpException : Exception
{
    public QueueSharpException()
    {
        
    }

    public QueueSharpException(string? message) : base(message)
    {
    }
}
