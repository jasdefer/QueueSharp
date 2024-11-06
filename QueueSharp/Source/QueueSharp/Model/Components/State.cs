using QueueSharp.Model.Events;
using System.Collections.Frozen;

namespace QueueSharp.Model.Components;
internal class State
{

    private readonly Dictionary<Individual, Dictionary<string, Dictionary<int, NodeVisit>>> _nodeVisits = [];
    internal required PriorityQueue<IEvent, int> EventQueue { get; init; }
    internal required FrozenDictionary<string, SimulationNode> Nodes { get; init; }
    internal required FrozenDictionary<string, Node> InputNodesById { get; init; }
    internal IEnumerable<NodeVisitRecord> NodeVisitRecords => _nodeVisits
        .SelectMany(x => x.Value
            .SelectMany(y => y.Value
                .Select(z => Map(x.Key, Nodes[y.Key], z.Key, z.Value))));

    internal void AddArrival(Individual individual, SimulationNode node, int time)
    {
        Dictionary<string, Dictionary<int, NodeVisit>>? record;
        bool containsIndividual = _nodeVisits.TryGetValue(individual, out record);
        if (!containsIndividual)
        {
            record = [];
            _nodeVisits.Add(individual, record);
        }
        Dictionary<int, NodeVisit>? visits;
        bool containsNode = record!.TryGetValue(node.Id, out visits);
        if (!containsNode)
        {
            visits = [];
            record.Add(node.Id, visits);
        }

        visits!.Add(time, new NodeVisit()
        {
            QueueSizeAtArrival = node.Queue.Count
        });
    }

    internal void StartService(Individual individual, SimulationNode node, int arrivalTime, int time, int server)
    {
        _nodeVisits[individual][node.Id][arrivalTime].ServiceStartTime = time;
        _nodeVisits[individual][node.Id][arrivalTime].Server = server;
    }

    internal void CompleteService(Individual individual, SimulationNode node, int arrivalTime, int time)
    {
        _nodeVisits[individual][node.Id][arrivalTime].ServiceEndTime = time;
    }

    internal void Reject(Individual individual, SimulationNode node, int arrivalTime, RejectionReason rejectionReason, int? startServiceTime = null)
    {
        _nodeVisits[individual][node.Id][arrivalTime].RejectionReason = rejectionReason;
        _nodeVisits[individual][node.Id][arrivalTime].ServiceStartTime = startServiceTime;
    }

    internal void Exit(Individual individual, SimulationNode node, int arrivalTime, int time)
    {
        _nodeVisits[individual][node.Id][arrivalTime].ExitTime = time;
        _nodeVisits[individual][node.Id][arrivalTime].QueueSizeAtExit = node.Queue.Count;
    }

    internal void ExitToDestination(Individual individual, SimulationNode node, int arrivalTime, int time, SimulationNode destination)
    {
        _nodeVisits[individual][node.Id][arrivalTime].ExitTime = time;
        _nodeVisits[individual][node.Id][arrivalTime].QueueSizeAtExit = node.Queue.Count;
        _nodeVisits[individual][node.Id][arrivalTime].DestinationNodeId = destination.Id;
    }

    private static NodeVisitRecord Map(Individual individual, SimulationNode node, int arrival, NodeVisit nodeVisit)
    {
        if (nodeVisit.RejectionReason == RejectionReason.QueueFull)
        {
            return new RejectionAtArrival(individual,
                node.Id,
                arrival,
                nodeVisit.QueueSizeAtArrival!.Value);
        }
        else if (nodeVisit.RejectionReason == RejectionReason.CannotCompleteService ||
            nodeVisit.RejectionReason == RejectionReason.CannotSelectServer)
        {
            return new RejectionAtStartService(individual,
                node.Id,
                arrival,
                nodeVisit.ServiceStartTime!.Value,
                nodeVisit.QueueSizeAtArrival!.Value);
        }

        return new NodeServiceRecord(individual,
            node.Id,
            arrival,
            nodeVisit.ServiceStartTime!.Value,
            nodeVisit.ServiceEndTime!.Value,
            nodeVisit.ExitTime!.Value,
            nodeVisit.DestinationNodeId,
            nodeVisit.QueueSizeAtArrival!.Value,
            nodeVisit.QueueSizeAtExit!.Value,
            nodeVisit.Server!.Value);
    }

    internal void CancelSimulation()
    {
        foreach (SimulationNode node in Nodes.Values)
        {
            foreach ((Individual individual, int arrivalTime) in node.Queue)
            {
                _nodeVisits[individual][node.Id].Remove(arrivalTime);
            }
            foreach (Individual? individual in node.ServingIndividuals)
            {
                if (individual is null)
                {
                    continue;
                }
                _nodeVisits[individual][node.Id].Remove(_nodeVisits[individual][node.Id].Max(x => x.Key));
            }
        }
    }

    private class NodeVisit
    {
        internal int? ServiceStartTime { get; set; }
        internal int? ServiceEndTime { get; set; }
        internal int? ExitTime { get; set; }
        internal string? DestinationNodeId { get; set; }
        internal int? QueueSizeAtArrival { get; set; }
        internal int? QueueSizeAtExit { get; set; }
        internal int? Server { get; set; }
        internal RejectionReason? RejectionReason { get; set; }
    }
}
