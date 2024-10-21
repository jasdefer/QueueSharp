using QueueSharp.Model.Events;

namespace QueueSharp.Model.Components;

/// <summary>
/// The result of processing an event is stored in an <see cref="ActivityLog"/>.
/// </summary>
internal abstract record ActivityLog(int Timestamp);

internal record BaulkingLog(int Timestamp, Individual Individual, Node Node) : ActivityLog(Timestamp);
internal record EnqueueLog(int Timestamp, Individual Individual, Node Node) : ActivityLog(Timestamp);
internal record StartServiceLog(CompleteServiceEvent CompleteServiceEvent) : ActivityLog(CompleteServiceEvent.ServiceStart);
internal record AddToOverflowQueueLog(int Timestamp, Individual Individual, Node Origin, Node Destination) : ActivityLog(Timestamp);