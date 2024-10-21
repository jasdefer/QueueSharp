using QueueSharp.Model.Components;
using QueueSharp.Model.DurationDistribution;
using QueueSharp.Model.Events;
using QueueSharp.Model.Exceptions;
using QueueSharp.Model.Routing;
using System.Collections.Immutable;

namespace QueueSharp.Logic;
internal class Simulation
{
    private State? _state;
    private int _time = 0;
    private readonly SimulationSettings _simulationSettings;

    internal Simulation(IEnumerable<Cohort> cohorts,
        SimulationSettings? simulationSettings = null)
    {
        Cohorts = cohorts.ToImmutableArray();
        _simulationSettings = simulationSettings ?? new SimulationSettings(MaxTime: null);
    }

    public ImmutableArray<Cohort> Cohorts { get; }

    internal ImmutableArray<ActivityLog> Start(CancellationToken? cancellationToken = null)
    {
        Initialize();
        ProcessEvents(cancellationToken ?? CancellationToken.None);
        return _state!.ActivityLogs.ToImmutableArray();
    }

    private void Initialize()
    {
        _time = 0;

        // Extract all distinct nodes from the cohorts
        ImmutableArray<Node> nodes = Cohorts
            .SelectMany(x => x.PropertiesByNode.Keys)
            .Distinct()
            .ToImmutableArray();

        _state = new State()
        {
            EventList = new EventList(),
            Node = nodes
        };

        // Create the initial arrival event for each cohort and node
        foreach (Cohort cohort in Cohorts)
        {
            foreach ((Node node, NodeProperties nodeProperties) in cohort.PropertiesByNode)
            {
                CreateArrivalEvent(cohort, node, true);
            }
        }
    }

    internal void ProcessEvents(CancellationToken cancellationToken)
    {
        while (!_state!.EventList.IsEmpty)
        {
            IEvent currentEvent = _state.EventList.Dequeue();
            if (_simulationSettings.MaxTime < currentEvent.Timestamp)
            {
                return;
            }
            EventLog eventLog = new EventLog(currentEvent);
            _state.ActivityLogs.Add(eventLog);
            ProcessEvent(currentEvent);

            if (cancellationToken.IsCancellationRequested)
            {
                return;
            }
        }
    }

    private void ProcessEvent(IEvent @event)
    {
        _time = @event.Timestamp;
        switch (@event)
        {
            case ArrivalEvent arrivalEvent:
                IndividualArrives(arrivalEvent.Individual, arrivalEvent.Node);
                CreateArrivalEvent(arrivalEvent.Individual.Cohort, arrivalEvent.Node);
                return;
            case CompleteServiceEvent completeServiceEvent:
                CompleteService(completeServiceEvent.Individual, completeServiceEvent.Node, completeServiceEvent.Server);
                return;
            default:
                throw new NotImplementedEventException(@event.GetType().Name);
        };
    }

    private void IndividualArrives(Individual individual, Node destination)
    {
        if (destination.IsQueueEmpty)
        {
            TryServerIndividual(individual, destination);
            return;
        }

        if (destination.IsQueueFull)
        {
            BaulkIndividual(individual, destination);
            return;
        }
        EnqueueIndividual(individual, destination);
    }

    private void CreateArrivalEvent(Cohort cohort, Node destination, bool isInitialArrival = false)
    {
        NodeProperties nodeProperties = cohort.PropertiesByNode[destination];
        bool hasArrival = nodeProperties.DurationDistributionSelector.TryGetArrivalTime(time:
                    _time,
                    out int? arrival,
                    isInitialArrival: isInitialArrival);
        if (!hasArrival)
        {
            return;
        }
        Individual individual = cohort.CreateIndividual();
        ArrivalEvent arrivalEvent = new(arrival!.Value, individual, destination);
        _state!.EventList.Insert(arrivalEvent);
    }

    private void CompleteService(Individual individual, Node origin, int server)
    {
        bool individualLeavesOrigin = TryLeaveIndividual(individual, origin);
        if (!individualLeavesOrigin)
        {
            return;
        }

        // Individual is leaving the origin node
        IndividualLeavesNode(server, origin);

        // Handle Overflow queueu
        if (origin.OverflowQueue.Count == 0)
        {
            return;
        }
        Individual overflowIndividual = origin.OverflowQueue[0];
        origin.OverflowQueue.RemoveAt(0);
        for (int i = 0; i < origin.ServingIndividuals.Length; i++)
        {
            if (origin.ServingIndividuals[i] == individual)
            {
                // ToDo: Performance improvement by caching the index of the server
                origin.ServingIndividuals[i] = null;
                break;
            }
        }
        IndividualArrives(overflowIndividual, origin);
    }

    private bool TryLeaveIndividual(Individual individual, Node origin)
    {
        RoutingDecision routingDecision = individual.Cohort.Routing.RouteAfterService(origin, _state!);
        switch (routingDecision)
        {
            case ExitSystem:
                return true;
            case SeekDestination seekDestination:
                // The individual tries to seek the chosen destination
                if (!seekDestination.Destination.IsQueueFull)
                {
                    IndividualArrives(individual, seekDestination.Destination);
                    return true;
                }
                // Queue at destination is full
                switch (seekDestination.QueueIsFullBehavior)
                {
                    case QueueIsFullBehavior.Baulk:
                        BaulkIndividual(individual, origin);
                        return true;
                    case QueueIsFullBehavior.WaitAndBlockCurrentServer:
                        AddIndividualToOverflowQueue(individual, origin, seekDestination.Destination);
                        return false;
                    default:
                        throw new NotImplementedEventException(seekDestination.QueueIsFullBehavior.ToString());

                }
            default:
                throw new NotImplementedEventException(routingDecision.GetType().Name);
        }
    }

    private void TryServerIndividual(Individual individual, Node destination)
    {
        NodeProperties nodeProperties = individual.Cohort.PropertiesByNode[destination];
        bool canSelectServer = nodeProperties.ServerSelector.CanSelectServer(destination, out int? selectedServer);
        if (!canSelectServer)
        {
            if (destination.IsQueueFull)
            {
                BaulkIndividual(individual, destination);
                return;
            }
            EnqueueIndividual(individual, destination);
            return;
        }
        if (selectedServer >= destination.ServerCount ||
            destination.ServingIndividuals[selectedServer!.Value] is not null)
        {
            throw new ImplausibleStateException($"Cannot select the server {selectedServer} for the Node {destination.Id}.");
        }
        StartService(individual, destination, nodeProperties.ServiceDurationSelector, selectedServer);
    }

    private void StartService(Individual individual, Node node, DurationDistributionSelector serviceDurationSelector, int? selectedServer)
    {
        node.ServingIndividuals[selectedServer!.Value] = individual;
        bool canCompleteService = serviceDurationSelector.TryGetArrivalTime(_time, out int? serviceCompleted, false);
        if (!canCompleteService)
        {
            BaulkIndividual(individual, node);
            return;
        }
        CompleteServiceEvent completeServiceEvent = new(Timestamp: serviceCompleted!.Value,
            Server: selectedServer.Value,
            Individual: individual,
            Node: node);
        _state!.EventList.Insert(completeServiceEvent);
        StartServiceLog startServiceLog = new StartServiceLog(_time, completeServiceEvent);
        _state.ActivityLogs.Add(startServiceLog);
    }

    private void IndividualLeavesNode(int server, Node origin)
    {
        if (origin.IsQueueEmpty)
        {
            origin.ServingIndividuals[server] = null;
            return;
        }

        Individual nextIndividual = origin.Queue[0];
        origin.Queue.RemoveAt(0);
        StartService(nextIndividual, origin, nextIndividual.Cohort.PropertiesByNode[origin].ServiceDurationSelector, server);
    }

    private void AddIndividualToOverflowQueue(Individual individual, Node origin, Node destination)
    {
        destination.OverflowQueue.Add(individual);
        AddToOverflowQueueLog overflowLog = new AddToOverflowQueueLog(_time, individual, origin, destination);
        _state!.ActivityLogs.Add(overflowLog);
    }

    private void BaulkIndividual(Individual individual, Node node)
    {
        BaulkingLog baulkingLog = new(_time, individual, node);
        _state!.ActivityLogs.Add(baulkingLog);
    }

    private void EnqueueIndividual(Individual individual, Node node)
    {
        node.Queue.Add(individual);
        EnqueueLog enqueueLog = new(_time, individual, node, node.Queue.Count);
        _state!.ActivityLogs.Add(enqueueLog);
    }
}
