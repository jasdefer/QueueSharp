using QueueSharp.Model.Components;
using QueueSharp.Model.DurationDistribution;
using QueueSharp.Model.Events;
using QueueSharp.Model.Exceptions;
using QueueSharp.Model.Routing;
using System.Collections.Immutable;

namespace QueueSharp.Logic;
public class Simulation
{
    private State? _state;
    private int _time = 0;
    private readonly SimulationSettings _simulationSettings;

    public Simulation(IEnumerable<Cohort> cohorts,
        SimulationSettings? simulationSettings = null)
    {
        Cohorts = cohorts.ToImmutableArray();
        _simulationSettings = simulationSettings ?? new SimulationSettings(MaxTime: null);
    }

    public ImmutableArray<Cohort> Cohorts { get; }

    public IEnumerable<NodeVisitRecord> Start(CancellationToken? cancellationToken = null)
    {
        _time = 0;

        Initialize();
        ProcessEvents(cancellationToken ?? CancellationToken.None);
        return _state!.NodeVisitRecords;
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
            Nodes = nodes
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

    private void ProcessEvents(CancellationToken cancellationToken)
    {
        while (!_state!.EventList.IsEmpty)
        {
            IEvent currentEvent = _state.EventList.Dequeue();
            if (_simulationSettings.MaxTime < currentEvent.Timestamp)
            {
                _state.CancelSimulation();
                return;
            }
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
        if (_time > int.MaxValue - 1000)
        {
            // ToDo: Find better bounds for 1000
            throw new InvalidInputException("Simulation time is too long.");
        }
        switch (@event)
        {
            case ArrivalEvent arrivalEvent:
                IndividualArrives(arrivalEvent.Individual, arrivalEvent.Node);
                CreateArrivalEvent(arrivalEvent.Individual.Cohort, arrivalEvent.Node);
                return;
            case CompleteServiceEvent completeServiceEvent:
                CompleteService(completeServiceEvent.Individual, completeServiceEvent.Node, completeServiceEvent.ArrivalTime, completeServiceEvent.Server);
                return;
            default:
                throw new NotImplementedEventException(@event.GetType().Name);
        };
    }

    private void IndividualArrives(Individual individual, Node destination)
    {
        _state!.AddArrival(individual, destination, _time);
        if (destination.IsQueueEmpty)
        {
            TryServerIndividual(individual, destination, _time);
            return;
        }

        if (destination.IsQueueFull)
        {
            _state!.Reject(individual, destination, _time, RejectionReason.QueueFull);
            return;
        }
        destination.Queue.Add((individual, _time));
    }

    private void CreateArrivalEvent(Cohort cohort, Node destination, bool isInitialArrival = false)
    {
        NodeProperties nodeProperties = cohort.PropertiesByNode[destination];
        bool hasArrival = nodeProperties.ArrivalDistributionSelector.TryGetNextTime(time:
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

    private void CompleteService(Individual individual, Node origin, int arrivalTime, int server)
    {
        _state!.CompleteService(individual, origin, arrivalTime, _time);
        bool individualLeavesOrigin = TryLeaveIndividual(individual, origin, arrivalTime, server);
        if (!individualLeavesOrigin)
        {
            return;
        }

        // Individual is leaving the origin node
        IndividualLeftNode(server, origin);

        // Handle Overflow queue
        // This overflow handling can be chained if multiple blocked nodes are connected
        Queue<Node> overflowHandling = [];
        overflowHandling.Enqueue(origin);
        while (overflowHandling.Count > 0)
        {
            Node nodeWithOverflowQueue = overflowHandling.Dequeue();
            if (nodeWithOverflowQueue.OverflowQueue.Count == 0)
            {
                break;
            }

            Overflow overflow = nodeWithOverflowQueue.OverflowQueue[0];
            nodeWithOverflowQueue.OverflowQueue.RemoveAt(0);
            IndividualArrives(overflow.Individual, nodeWithOverflowQueue);

            IndividualLeftNode(overflow.BlockedServer, overflow.BlockedNode);
            // The node from which the overflow is coming from, might now accept also a new overflowing individual
            // Check this in the next iteration of this queue
            overflowHandling.Enqueue(overflow.BlockedNode);
            _state.Exit(overflow.Individual, overflow.BlockedNode, overflow.ArrivalTime, _time);
        }
    }

    private bool TryLeaveIndividual(Individual individual, Node origin, int arrivalTime, int server)
    {
        RoutingDecision routingDecision = individual.Cohort.Routing.RouteAfterService(origin, _state!.Nodes);
        switch (routingDecision)
        {
            case ExitSystem:
                _state!.Exit(individual, origin, arrivalTime, _time);
                return true;
            case SeekDestination seekDestination:
                // The individual tries to seek the chosen destination
                if (!seekDestination.Destination.IsQueueFull)
                {
                    _state!.ExitToDestination(individual, origin, arrivalTime, _time, seekDestination.Destination);
                    IndividualArrives(individual, seekDestination.Destination);
                    return true;
                }
                // Queue at destination is full
                switch (seekDestination.QueueIsFullBehavior)
                {
                    case QueueIsFullBehavior.RejectIndividual:
                        _state!.Exit(individual, origin, arrivalTime, _time);
                        _state!.AddArrival(individual, seekDestination.Destination, _time);
                        _state!.Reject(individual, seekDestination.Destination, _time, RejectionReason.QueueFull);
                        return true;
                    case QueueIsFullBehavior.WaitAndBlockCurrentServer:
                        Overflow overflow = new(individual, origin, server, arrivalTime);
                        seekDestination.Destination.OverflowQueue.Add(overflow);
                        return false;
                    default:
                        throw new NotImplementedEventException(seekDestination.QueueIsFullBehavior.ToString());

                }
            default:
                throw new NotImplementedEventException(routingDecision.GetType().Name);
        }
    }

    private void TryServerIndividual(Individual individual, Node destination, int arrivalTime)
    {
        NodeProperties nodeProperties = individual.Cohort.PropertiesByNode[destination];
        bool canSelectServer = nodeProperties.ServerSelector.CanSelectServer(destination, out int? selectedServer);
        if (!canSelectServer)
        {
            if (destination.IsQueueFull)
            {
                _state!.Reject(individual, destination, arrivalTime, RejectionReason.CannotSelectServer, _time);
                return;
            }
            destination.Queue.Add((individual, arrivalTime));
            return;
        }
        if (selectedServer >= destination.ServerCount ||
            destination.ServingIndividuals[selectedServer!.Value] is not null)
        {
            throw new ImplausibleStateException($"Cannot select the server {selectedServer} for the Node {destination.Id}.");
        }
        TryStartService(individual, destination, nodeProperties.ServiceDurationSelector, arrivalTime, selectedServer.Value);
    }

    private bool TryStartService(Individual individual, Node node, DurationDistributionSelector serviceDurationSelector, int arrivalTime, int selectedServer)
    {
        node.ServingIndividuals[selectedServer] = individual;
        bool canCompleteService = serviceDurationSelector.TryGetNextTime(_time, out int? serviceCompleted, false);
        if (!canCompleteService)
        {
            _state!.Reject(individual, node, arrivalTime, RejectionReason.CannotCompleteService, _time);
            return false;
        }
        CompleteServiceEvent completeServiceEvent = new(Timestamp: serviceCompleted!.Value,
            ArrivalTime: arrivalTime,
            Server: selectedServer,
            Individual: individual,
            Node: node);
        _state!.EventList.Insert(completeServiceEvent);
        _state!.StartService(individual, node, arrivalTime, _time, selectedServer);
        return true;
    }

    private void IndividualLeftNode(int server, Node origin)
    {
        while (!origin.IsQueueEmpty)
        {
            // Try to start the service of the next individual
            // If the individual is rejected, try the next individual
            // If the individual can get served, break this loop
            (Individual nextIndividual, int nextIndividualsArrivalTime) = origin.Queue[0];
            origin.Queue.RemoveAt(0);
            bool canStartService = TryStartService(nextIndividual, origin, nextIndividual.Cohort.PropertiesByNode[origin].ServiceDurationSelector, nextIndividualsArrivalTime, server);
            if (canStartService)
            {
                return;
            }
        }

        origin.ServingIndividuals[server] = null;
        return;
    }

    public void ClearState()
    {
        if (_state is null)
        {
            return;
        }
        foreach (Node node in _state.Nodes)
        {
            node.Queue.Clear();
            node.OverflowQueue.Clear();
            node.SetServerCount(node.ServerCount);
        }
    }
}
