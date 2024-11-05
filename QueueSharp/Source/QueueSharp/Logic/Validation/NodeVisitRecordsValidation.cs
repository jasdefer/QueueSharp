using QueueSharp.Model.Components;
using QueueSharp.Model.Exceptions;

namespace QueueSharp.Logic.Validation;
/// <summary>
/// Check the plausibility of node visit records to check if the simulation made any mistakes
/// </summary>
internal class NodeVisitRecordsValidation
{
    internal static void Validate(IEnumerable<NodeVisitRecord> nodeVisitRecords)
    {
        foreach (var group in nodeVisitRecords.OfType<NodeServiceRecord>().GroupBy(x => new { x.NodeId, x.Server }))
        {
            int? previousExitTime = null;
            NodeServiceRecord? previous = null;
            IOrderedEnumerable<NodeServiceRecord> sortedServiceRecords = group
                .OrderBy(x => x.ServiceStartTime)
                .ThenBy(x => x.ExitTime);
            foreach (NodeServiceRecord nodeServiceRecord in sortedServiceRecords)
            {
                if (previousExitTime > nodeServiceRecord.ServiceStartTime)
                {
                    throw new ImplausibleStateException("A server cannot process an individual before completing the previous individual.");
                }
                previousExitTime = nodeServiceRecord.ExitTime;
                previous = nodeServiceRecord;
            }
        }

        foreach (IGrouping<Individual, NodeVisitRecord> group in nodeVisitRecords.GroupBy(x => x.Individual))
        {
            int? previousExitTime = null;
            NodeVisitRecord? previousRecord = null;
            bool previousEventWasRejection = false;
            foreach (NodeVisitRecord? nodeVisitRecord in group.OrderBy(x => x.ArrivalTime))
            {
                if (previousEventWasRejection)
                {
                    throw new ImplausibleStateException("No records can occur after a rejection record, because the individual left the system.");
                }
                if (previousExitTime is null)
                {
                    previousExitTime = nodeVisitRecord.ArrivalTime;
                }
                if (previousExitTime != nodeVisitRecord.ArrivalTime)
                {
                    throw new ImplausibleStateException("The arrival time of a node service must be equal to the exit time of the previous node service");
                }

                if (nodeVisitRecord is NodeServiceRecord nodeServiceRecord)
                {
                    Validate(nodeServiceRecord);
                    previousExitTime = nodeServiceRecord.ExitTime;
                }
                else
                {
                    // The current event is a rejection event, if it is not a service record
                    previousEventWasRejection = true;
                }
                previousRecord = nodeVisitRecord;
            }
        }
    }

    internal static void Validate(NodeServiceRecord nodeServiceRecord)
    {
        if (nodeServiceRecord.QueueSizeAtArrival < 0)
        {
            throw new ImplausibleStateException("Queue size at arrival cannot be negative.");
        }
        if (nodeServiceRecord.QueueSizeAtExit < 0)
        {
            throw new ImplausibleStateException("Queue size at exit cannot be negative.");
        }
        if (nodeServiceRecord.ArrivalTime > nodeServiceRecord.ServiceStartTime)
        {
            throw new ImplausibleStateException("Arrival cannot be later than the start service time.");
        }

        if (nodeServiceRecord.ServiceStartTime > nodeServiceRecord.ServiceEndTime)
        {
            throw new ImplausibleStateException("The start service time cannot be later than the service end time.");
        }

        if (nodeServiceRecord.ServiceEndTime > nodeServiceRecord.ExitTime)
        {
            throw new ImplausibleStateException("The service end time cannot be later than the exit time.");
        }
    }
}
