using QueueSharp.Model.Components;
using QueueSharp.Model.DurationDistribution;
using QueueSharp.Model.Events;
using QueueSharp.Model.Exceptions;
using System.Collections.Immutable;
using System.Xml.Linq;

namespace QueueSharp.Logic;
internal class Simulation
{
    private readonly ImmutableArray<Cohort> _cohorts;
    private State _state;
    private int _time = 0;

    internal Simulation(IEnumerable<Cohort> cohorts)
    {
        _cohorts = cohorts.ToImmutableArray();
    }

    internal void Start()
    {
        Initialize();
        ProcessEvents();
    }

    private void Initialize()
    {
        _time = 0;

        // Extract all distinct nodes from the cohorts
        ImmutableArray<Node> nodes = _cohorts
            .SelectMany(x => x.PropertiesByNode.Keys)
            .Distinct()
            .ToImmutableArray();

        _state = new State()
        {
            EventList = new EventList(),
            Node = nodes
        };

        // Create the initial arrival event for each cohort and node
        foreach (Cohort cohort in _cohorts)
        {
            foreach ((Node node, NodeProperties nodeProperties) in cohort.PropertiesByNode)
            {
                bool hasArrival = nodeProperties.DurationDistributionSelector.TryGetArrivalTime(time:
                    0,
                    out int? arrival,
                    isInitialArrival: true);
                if (!hasArrival)
                {
                    continue;
                }
                Individual individual = cohort.CreateIndividual();
                ArrivalEvent arrivalEvent = new(arrival!.Value, individual, node);
                _state.EventList.Insert(arrivalEvent);
            }
        }
    }

    internal void ProcessEvents()
    {
        while (!_state.EventList.IsEmpty)
        {
            IEvent currentEvent = _state.EventList.Dequeue();
            ProcessEvent(currentEvent);
        }
    }

    private void EnqueueIndividual(Individual individual, Node node)
    {
        node.Queue.Add(individual);
        EnqueueLog enqueueLog = new(_time, individual, node);
        _state.ActivityLogs.Add(enqueueLog);
    }

    private void ProcessEvent(IEvent @event)
    {
        switch (@event)
        {
            case ArrivalEvent arrivalEvent:
                ProcessArrival(arrivalEvent);
                return;
            case CompleteServiceEvent completeServiceEvent:
                ProcessServiceCompletion(completeServiceEvent);
                return;
            default:
                throw new NotImplementedEventException(@event.GetType().Name);
        };
    }

    private void ProcessArrival(ArrivalEvent arrivalEvent)
    {
        Node node = arrivalEvent.Node;
        if (node.Queue.Count != 0)
        {
            if (node.QueueCapacity <= node.Queue.Count)
            {
                BaulkingLog baulkingLog = new(_time, arrivalEvent.Individual, node);
                _state.ActivityLogs.Add(baulkingLog);
                return;
            }
            EnqueueIndividual(arrivalEvent.Individual, node);

            return;
        }

        NodeProperties nodeProperties = arrivalEvent.Individual.Cohort.PropertiesByNode[node];
        bool canSelectServer = nodeProperties.ServerSelector.CanSelectServer(node, out int? selectedServer);
        if (!canSelectServer)
        {
            EnqueueIndividual(arrivalEvent.Individual, node);
            return;
        }

        if (selectedServer >= node.ServerCount ||
            node.IsServerIdle[selectedServer!.Value] == false)
        {
            throw new ImplausibleStateException($"Cannot select the server {selectedServer} for the Node {node.Id}.");
        }

        StartService(arrivalEvent.Individual, node, nodeProperties.ServiceDurationSelector, selectedServer);
    }

    private void StartService(Individual individual, Node node, DurationDistributionSelector serviceDurationSelector, int? selectedServer)
    {
        node.IsServerIdle[selectedServer!.Value] = false;
        bool canCompleteService = serviceDurationSelector.TryGetArrivalTime(_time, out int? serviceCompleted, false);
        if (!canCompleteService)
        {
            BaulkingLog baulkingLog = new(_time, individual, node);
            _state.ActivityLogs.Add(baulkingLog);
            return;
        }
        CompleteServiceEvent completeServiceEvent = new(Timestamp: serviceCompleted!.Value,
            ServiceStart: _time,
            Server: selectedServer.Value,
            Individual: individual,
            Node: node);
        _state.EventList.Insert(completeServiceEvent);
        StartServiceLog startServiceLog = new StartServiceLog(completeServiceEvent);
        _state.ActivityLogs.Add(startServiceLog);
    }

    private void ProcessServiceCompletion(CompleteServiceEvent completeServiceEvent)
    {
        Individual individual = completeServiceEvent.Individual;
        Node origin = completeServiceEvent.Node;
        bool hasDestination = individual.Cohort.Routing.TryGetDestination(origin, _state, out Node? destination);
        if (hasDestination)
        {
            if (destination!.IsQueueFull)
            {
                destination.OverflowQueue.Add(individual);
                AddToOverflowQueueLog overflowLog = new AddToOverflowQueueLog(_time, individual, origin, destination);
                _state.ActivityLogs.Add(overflowLog);
                return;
            }
            ArrivalEvent arrivalEvent = new(_time, individual, destination);
            _state.EventList.Insert(arrivalEvent);
        }

        // Individual is leaving the origin node
        IndividualLeavesNode(completeServiceEvent.Server, individual, origin);

        // Handle Overflow queueu
        if (origin.OverflowQueue.Count == 0)
        {
            return;
        }
        Individual overflowIndividual = origin.OverflowQueue[0];
        origin.OverflowQueue.RemoveAt(0);
        ArrivalEvent overflowArrival = new(_time, overflowIndividual, origin);
        IndividualLeavesNode()
    }

    private void IndividualLeavesNode(int server, Individual individual, Node origin)
    {
        if (origin.IsQueueEmpty)
        {
            origin.ServingIndividuals[server] = null;
            return;
        }

        Individual nextIndividual = origin.Queue[0];
        origin.Queue.RemoveAt(0);
        NodeProperties nodeProperties = nextIndividual.Cohort.PropertiesByNode[origin];
        bool canSelectServer = nodeProperties.ServerSelector.CanSelectServer(origin, out int? selectedServer);
        if (!canSelectServer)
        {
            BaulkingLog baulkingLog = new(_time, individual, origin);
            _state.ActivityLogs.Add(baulkingLog);
        }
        else
        {
            StartService(nextIndividual, origin, nodeProperties.ServiceDurationSelector, selectedServer);
        }
    }
}
