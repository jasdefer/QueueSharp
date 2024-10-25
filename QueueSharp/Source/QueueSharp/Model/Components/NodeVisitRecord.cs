using System.Diagnostics;

namespace QueueSharp.Model.Components;

[DebuggerDisplay("{Individual.Id} of {Individual.Cohort.Id} at {Node.Id} arrived at {ArrivalTime}")]

public abstract record NodeVisitRecord(Individual Individual,
    Node Node,
    int ArrivalTime,
    int QueueSizeAtArrival);

[DebuggerDisplay("Baulking from {Individual.Id} of {Individual.Cohort.Id} at {Node.Id} arrived at {ArrivalTime}")]
public sealed record BaulkingAtArrival(Individual Individual,
    Node Node,
    int ArrivalTime,
    int QueueSizeAtArrival) : NodeVisitRecord(Individual, Node, ArrivalTime, QueueSizeAtArrival);

public abstract record NodeServiceStartRecord(Individual Individual,
    Node Node,
    int ArrivalTime,
    int ServiceStartTime,
    int QueueSizeAtArrival) : NodeVisitRecord(Individual, Node, ArrivalTime, QueueSizeAtArrival)
{
    public int WaitingDuration => ServiceStartTime - ArrivalTime;
};

[DebuggerDisplay("Baulking service from {Individual.Id} of {Individual.Cohort.Id} at {Node.Id} arrived at {ArrivalTime}")]
public sealed record BaulkingAtStartService(Individual Individual,
    Node Node,
    int ArrivalTime,
    int QueueSizeAtArrival,
    int ServiceStartTime) : NodeServiceStartRecord(Individual, Node, ArrivalTime, ServiceStartTime, QueueSizeAtArrival);

[DebuggerDisplay("{Individual.Id} of {Individual.Cohort.Id} at {Node.Id} arrived at {ArrivalTime} and exited at {ExitTime}")]
public sealed record NodeServiceRecord(Individual Individual,
    Node Node,
    int ArrivalTime,
    int ServiceStartTime,
    int ServiceEndTime,
    int ExitTime,
    Node? Destination,
    int QueueSizeAtArrival,
    int QueueSizeAtExit,
    int Server) : NodeServiceStartRecord(Individual, Node, ArrivalTime, ServiceStartTime, QueueSizeAtArrival)
{
    public int ServiceDuration => ServiceEndTime - ServiceStartTime;
    public int BlockDuration => ExitTime - ServiceEndTime;
}
