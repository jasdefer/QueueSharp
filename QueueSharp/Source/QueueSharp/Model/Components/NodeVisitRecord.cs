using System.Diagnostics;

namespace QueueSharp.Model.Components;

[DebuggerDisplay("{Individual.Id} of {Individual.Cohort.Id} at {Node.Id} arrived at {ArrivalTime}")]

public abstract record NodeVisitRecord(Individual Individual,
    string NodeId,
    int ArrivalTime,
    int QueueSizeAtArrival);

[DebuggerDisplay("Rejection from {Individual.Id} of {Individual.Cohort.Id} at {Node.Id} arrived at {ArrivalTime}")]
public sealed record RejectionAtArrival(Individual Individual,
    string NodeId,
    int ArrivalTime,
    int QueueSizeAtArrival) : NodeVisitRecord(Individual, NodeId, ArrivalTime, QueueSizeAtArrival);

public abstract record NodeServiceStartRecord(Individual Individual,
    string NodeId,
    int ArrivalTime,
    int ServiceStartTime,
    int QueueSizeAtArrival) : NodeVisitRecord(Individual, NodeId, ArrivalTime, QueueSizeAtArrival)
{
    public int WaitingDuration => ServiceStartTime - ArrivalTime;
};

[DebuggerDisplay("Reject service from {Individual.Id} of {Individual.Cohort.Id} at {Node.Id} arrived at {ArrivalTime}")]
public sealed record RejectionAtStartService(Individual Individual,
    string NodeId,
    int ArrivalTime,
    int QueueSizeAtArrival,
    int ServiceStartTime) : NodeServiceStartRecord(Individual, NodeId, ArrivalTime, ServiceStartTime, QueueSizeAtArrival);

[DebuggerDisplay("{Individual.Id} of {Individual.Cohort.Id} at {Node.Id} arrived at {ArrivalTime}, started service at {ServiceStartTime} and exited at {ExitTime}")]
public sealed record NodeServiceRecord(Individual Individual,
    string NodeId,
    int ArrivalTime,
    int ServiceStartTime,
    int ServiceEndTime,
    int ExitTime,
    string? DestinationNodeId,
    int QueueSizeAtArrival,
    int QueueSizeAtExit,
    int Server) : NodeServiceStartRecord(Individual, NodeId, ArrivalTime, ServiceStartTime, QueueSizeAtArrival)
{
    public int ServiceDuration => ServiceEndTime - ServiceStartTime;
    public int BlockDuration => ExitTime - ServiceEndTime;
}
