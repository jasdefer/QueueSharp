using System.Diagnostics;

namespace QueueSharp.Model.Components;

/// <summary>
/// Represents the base visit record for an individual arriving at a node.
/// </summary>
/// <param name="Individual">The individual for this record.</param>
/// <param name="NodeId">The node the individual arrived at.</param>
/// <param name="ArrivalTime">The time when the indivudal arrived at the node.</param>
/// <param name="QueueSizeAtArrival">The length of the queue at the node when the individual arrived.</param>
[DebuggerDisplay("{Individual.Id} of {Individual.Cohort.Id} at {Node.Id} arrived at {ArrivalTime}")]
public abstract record NodeVisitRecord(Individual Individual,
    string NodeId,
    int ArrivalTime,
    int QueueSizeAtArrival);

/// <summary>
/// The individual got rejected when arriving at the node.
/// </summary>
/// <param name="Individual">The individual for this record.</param>
/// <param name="NodeId">The node the individual arrived at.</param>
/// <param name="ArrivalTime">The time when the indivudal arrived at the node.</param>
/// <param name="QueueSizeAtArrival">The length of the queue at the node when the individual arrived.</param>
[DebuggerDisplay("Rejection from {Individual.Id} of {Individual.Cohort.Id} at {Node.Id} arrived at {ArrivalTime}")]
public sealed record RejectionAtArrival(Individual Individual,
    string NodeId,
    int ArrivalTime,
    int QueueSizeAtArrival) : NodeVisitRecord(Individual, NodeId, ArrivalTime, QueueSizeAtArrival);

/// <summary>
/// Represents the base visit record for an individual which tried to start the service at a node.
/// </summary>
/// <param name="Individual">The individual for this record.</param>
/// <param name="NodeId">The node the individual arrived at.</param>
/// <param name="ArrivalTime">The time when the indivudal arrived at the node.</param>
/// <param name="QueueSizeAtArrival">The length of the queue at the node when the individual arrived.</param>
/// <param name="ServiceStartTime">The time when the service of the individual started.</param>
public abstract record NodeServiceStartRecord(Individual Individual,
    string NodeId,
    int ArrivalTime,
    int ServiceStartTime,
    int QueueSizeAtArrival) : NodeVisitRecord(Individual, NodeId, ArrivalTime, QueueSizeAtArrival)
{
    /// <summary>
    /// The time spend in the queue waiting for the service to start.
    /// </summary>
    public int WaitingDuration => ServiceStartTime - ArrivalTime;
};

/// <summary>
/// Represents the base visit record for an individual which got rejected after trying to start the service.
/// </summary>
/// <param name="Individual">The individual for this record.</param>
/// <param name="NodeId">The node the individual arrived at.</param>
/// <param name="ArrivalTime">The time when the indivudal arrived at the node.</param>
/// <param name="QueueSizeAtArrival">The length of the queue at the node when the individual arrived.</param>
/// <param name="ServiceStartTime">The time when the service of the individual started.</param>
[DebuggerDisplay("Reject service from {Individual.Id} of {Individual.Cohort.Id} at {Node.Id} arrived at {ArrivalTime}")]
public sealed record RejectionAtStartService(Individual Individual,
    string NodeId,
    int ArrivalTime,
    int QueueSizeAtArrival,
    int ServiceStartTime) : NodeServiceStartRecord(Individual, NodeId, ArrivalTime, ServiceStartTime, QueueSizeAtArrival);

/// <summary>
/// Represents the base visit record for an individual which got served at the node.
/// </summary>
/// <param name="Individual">The individual for this record.</param>
/// <param name="NodeId">The node the individual arrived at.</param>
/// <param name="ArrivalTime">The time when the indivudal arrived at the node.</param>
/// <param name="QueueSizeAtArrival">The length of the queue at the node when the individual arrived.</param>
/// <param name="ServiceStartTime">The time when the service of the individual started.</param>
/// <param name="DestinationNodeId">The id of the node the individual went after exiting this node. Is null, if the individual left the system.</param>
/// <param name="ExitTime">The time when the individual left the node. Can be larger than the <paramref name="ServiceEndTime"/>, if the individual blocked the current node because the queue at the destination node was full.</param>
/// <param name="QueueSizeAtExit">The length of the queue when the individual left the node.</param>
/// <param name="Server">The index of the server which served the individual.</param>
/// <param name="ServiceEndTime">The time when the service was completed.</param>
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
    /// <summary>
    /// The duration it took to serve the individual
    /// </summary>
    public int ServiceDuration => ServiceEndTime - ServiceStartTime;

    /// <summary>
    /// The individual can block the server of the node if the queue at the destination node was full.
    /// </summary>
    public int BlockDuration => ExitTime - ServiceEndTime;
}
