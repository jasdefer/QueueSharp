namespace QueueSharp.Model.Exceptions;
internal class InvalidInputException : QueueSharpException
{
    public InvalidInputException(string message) : base(message: message)
    {

    }
}