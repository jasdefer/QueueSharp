using QueueSharp.Model.Events;
using System.Diagnostics;

namespace QueueSharp.Model.Components;

/// <summary>
/// The result of processing an event is stored in an <see cref="ActivityLog"/>.
/// </summary>
internal abstract record ActivityLog(int Timestamp);
[DebuggerDisplay("{Event.Timestamp}: {Event.GetType().Name}")]
internal record EventLog(IEvent Event) : ActivityLog(Event.Timestamp);
[DebuggerDisplay("{Timestamp}: Start Service for {CompleteServiceEvent.Individual.Id}")]
internal record StartServiceLog(int Timestamp, CompleteServiceEvent CompleteServiceEvent) : ActivityLog(Timestamp);
[DebuggerDisplay("{Timestamp}: Baulking {Individual.Id}")]
internal record BaulkingLog(int Timestamp, Individual Individual, Node Node) : ActivityLog(Timestamp);
[DebuggerDisplay("{Timestamp}: Enqueue {Individual.Id}")]
internal record EnqueueLog(int Timestamp, Individual Individual, Node Node, int QueueLength) : ActivityLog(Timestamp);
[DebuggerDisplay("{Timestamp}: Add to overflow queue {Individual.Id}")]
internal record AddToOverflowQueueLog(int Timestamp, Individual Individual, Node Origin, Node Destination) : ActivityLog(Timestamp);