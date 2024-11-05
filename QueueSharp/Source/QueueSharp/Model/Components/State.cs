using QueueSharp.Model.Events;
using System.Collections.Generic;
using System.Collections.Immutable;

namespace QueueSharp.Model.Components;
internal class State
{

    private readonly Dictionary<Individual, Dictionary<Node, Dictionary<int, NodeVisit>>> _nodeVisits = [];
    internal required PriorityQueue<IEvent, int> EventQueue { get; init; }
    internal ImmutableArray<Node> Nodes { get; init; }
    internal IEnumerable<NodeVisitRecord> NodeVisitRecords => _nodeVisits
        .SelectMany(x => x.Value
            .SelectMany(y => y.Value
                .Select(z => Map(x.Key, y.Key, z.Key, z.Value))));

    internal void AddArrival(Individual individual, Node node, int time)
    {
        Dictionary<Node, Dictionary<int, NodeVisit>>? record;
        bool containsIndividual = _nodeVisits.TryGetValue(individual, out record);
        if (!containsIndividual)
        {
            record = [];
            _nodeVisits.Add(individual, record);
        }
        Dictionary<int, NodeVisit>? visits;
        bool containsNode = record!.TryGetValue(node, out visits);
        if (!containsNode)
        {
            visits = [];
            record.Add(node, visits);
        }

        visits!.Add(time, new NodeVisit()
        {
            QueueSizeAtArrival = node.Queue.Count
        });
    }

    internal void StartService(Individual individual, Node node, int arrivalTime, int time, int server)
    {
        _nodeVisits[individual][node][arrivalTime].ServiceStartTime = time;
        _nodeVisits[individual][node][arrivalTime].Server = server;
    }

    internal void CompleteService(Individual individual, Node node, int arrivalTime, int time)
    {
        _nodeVisits[individual][node][arrivalTime].ServiceEndTime = time;
    }

    internal void Reject(Individual individual, Node node, int arrivalTime, RejectionReason rejectionReason, int? startServiceTime = null)
    {
        _nodeVisits[individual][node][arrivalTime].RejectionReason = rejectionReason;
        _nodeVisits[individual][node][arrivalTime].ServiceStartTime = startServiceTime;
    }

    internal void Exit(Individual individual, Node node, int arrivalTime, int time)
    {
        _nodeVisits[individual][node][arrivalTime].ExitTime = time;
        _nodeVisits[individual][node][arrivalTime].QueueSizeAtExit = node.Queue.Count;
    }

    internal void ExitToDestination(Individual individual, Node node, int arrivalTime, int time, Node destination)
    {
        _nodeVisits[individual][node][arrivalTime].ExitTime = time;
        _nodeVisits[individual][node][arrivalTime].QueueSizeAtExit = node.Queue.Count;
        _nodeVisits[individual][node][arrivalTime].Destination = destination;
    }

    private static NodeVisitRecord Map(Individual individual, Node node, int arrival, NodeVisit nodeVisit)
    {
        if (nodeVisit.RejectionReason == RejectionReason.QueueFull)
        {
            return new RejectionAtArrival(individual,
                node,
                arrival,
                nodeVisit.QueueSizeAtArrival!.Value);
        }
        else if (nodeVisit.RejectionReason == RejectionReason.CannotCompleteService ||
            nodeVisit.RejectionReason == RejectionReason.CannotSelectServer)
        {
            return new RejectionAtStartService(individual,
                node,
                arrival,
                nodeVisit.ServiceStartTime!.Value,
                nodeVisit.QueueSizeAtArrival!.Value);
        }

        return new NodeServiceRecord(individual,
            node,
            arrival,
            nodeVisit.ServiceStartTime!.Value,
            nodeVisit.ServiceEndTime!.Value,
            nodeVisit.ExitTime!.Value,
            nodeVisit.Destination,
            nodeVisit.QueueSizeAtArrival!.Value,
            nodeVisit.QueueSizeAtExit!.Value,
            nodeVisit.Server!.Value);
    }

    internal void CancelSimulation()
    {
        foreach (Node node in Nodes)
        {
            foreach ((Individual individual, int arrivalTime) in node.Queue)
            {
                _nodeVisits[individual][node].Remove(arrivalTime);
            }
            foreach (Individual? individual in node.ServingIndividuals)
            {
                if (individual is null)
                {
                    continue;
                }
                _nodeVisits[individual][node].Remove(_nodeVisits[individual][node].Max(x => x.Key));
            }
        }
    }

    private class NodeVisit
    {
        internal int? ServiceStartTime { get; set; }
        internal int? ServiceEndTime { get; set; }
        internal int? ExitTime { get; set; }
        internal Node? Destination { get; set; }
        internal int? QueueSizeAtArrival { get; set; }
        internal int? QueueSizeAtExit { get; set; }
        internal int? Server { get; set; }
        internal RejectionReason? RejectionReason { get; set; }
    }
}
