namespace QueueSharp.Model.Exceptions;
internal class ImplausibleStateException : QueueSharpException
{
    public ImplausibleStateException(string? message = null) : base(message)
    {

    }
}
