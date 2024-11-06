namespace QueueSharp.Model.Components;

public enum RejectionReason
{
    /// <summary>
    /// Indicates that the queue is at maximum capacity, preventing further entries.
    /// </summary>
    QueueFull,
    /// <summary>
    /// Indicates that a suitable server could not be selected, possibly due to server unavailability or incompatibility.
    /// </summary>
    CannotSelectServer,
    /// <summary>
    /// Indicates that the service process cannot be completed, potentially due to resource constraints or interruptions.
    /// </summary>
    CannotCompleteService
}
