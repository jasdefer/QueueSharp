using QueueSharp.Model.Components;
using QueueSharp.Model.DurationDistribution;
using QueueSharp.Model.Events;
using QueueSharp.Model.Exceptions;
using QueueSharp.Model.Routing;
using System.Collections.Concurrent;
using System.Collections.Frozen;
using System.Collections.Immutable;

namespace QueueSharp.Logic;
public class Simulation
{
    private State? _state;
    private int _time = 0;
    private readonly Random _random;
    private readonly SimulationSettings _simulationSettings;

    public Simulation(IEnumerable<Cohort> cohorts,
        SimulationSettings? simulationSettings = null,
        int? randomSeed = null)
    {
        Cohorts = cohorts.ToImmutableArray();
        _simulationSettings = simulationSettings ?? new SimulationSettings(MaxTime: null);
        _random = randomSeed.HasValue
            ? new Random(randomSeed.Value)
            : new Random();
    }

    public ImmutableArray<Cohort> Cohorts { get; }

    public static async Task<IEnumerable<SimulationReport>> RunSimulationsInParallel(
        IEnumerable<Cohort> cohorts,
        int iterations,
        SimulationSettings? simulationSettings = null,
        int? minTime = null,
        int? maxTime = null,
        CancellationToken? cancellationToken = null)
    {
        ConcurrentBag<SimulationReport> reports = [];

        await Task.WhenAll(Enumerable.Range(0, iterations).Select(iteration => Task.Run(() =>
        {
            // Create a new Simulation instance for each iteration
            Simulation simulation = new Simulation(cohorts, simulationSettings, randomSeed: iteration);

            // Start the simulation and gather node visit records
            IEnumerable<NodeVisitRecord> nodeVisitRecords = simulation.Start(cancellationToken);

            // Generate a simulation report and add it to the concurrent collection
            SimulationReport report = SimulationAnalysis.GetSimulationReport(nodeVisitRecords, minTime, maxTime);
            reports.Add(report);
        }, cancellationToken ?? CancellationToken.None)));

        return reports;
    }

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
        ImmutableArray<SimulationNode> nodes = Cohorts
            .SelectMany(x => x.PropertiesByNode.Keys)
            .Distinct()
            .Select(x => new SimulationNode(x.Id, x.ServerCount, x.QueueCapacity))
            .ToImmutableArray();

        FrozenDictionary<string, Node> inputNodesById = Cohorts
            .SelectMany(x => x.PropertiesByNode.Keys)
            .Distinct()
            .ToFrozenDictionary(x => x.Id, x => x);

        _state = new State()
        {
            EventQueue = new(),
            Nodes = nodes.ToFrozenDictionary(x => x.Id, x => x),
            InputNodesById = inputNodesById
        };

        // Create the initial arrival event for each cohort and node
        foreach (Cohort cohort in Cohorts)
        {
            foreach ((Node node, NodeProperties nodeProperties) in cohort.PropertiesByNode)
            {
                CreateArrivalEvent(cohort, _state.Nodes[node.Id], true);
            }
        }
    }

    private void ProcessEvents(CancellationToken cancellationToken)
    {
        while (_state!.EventQueue.Count > 0)
        {
            IEvent currentEvent = _state.EventQueue.Dequeue();
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

    private void IndividualArrives(Individual individual, SimulationNode destination)
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
        destination.Queue.Enqueue((individual, _time));
    }

    private void CreateArrivalEvent(Cohort cohort, SimulationNode destination, bool isInitialArrival = false)
    {
        NodeProperties nodeProperties = cohort.GetPropertiesById(destination.Id);
        bool hasArrival = nodeProperties.ArrivalDistributionSelector.TryGetNextTime(
                    time: _time,
                    random: _random,
                    out int? arrival,
                    isInitialArrival: isInitialArrival);
        if (!hasArrival)
        {
            return;
        }
        Individual individual = cohort.CreateIndividual();
        ArrivalEvent arrivalEvent = new(arrival!.Value, individual, destination);
        _state!.EventQueue.Enqueue(arrivalEvent, arrivalEvent.Timestamp);
    }

    private void CompleteService(Individual individual, SimulationNode origin, int arrivalTime, int server)
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
        Queue<SimulationNode> overflowHandling = [];
        overflowHandling.Enqueue(origin);
        while (overflowHandling.Count > 0)
        {
            SimulationNode nodeWithOverflowQueue = overflowHandling.Dequeue();
            if (nodeWithOverflowQueue.OverflowQueue.Count == 0)
            {
                break;
            }

            Overflow overflow = nodeWithOverflowQueue.OverflowQueue.Dequeue();
            IndividualArrives(overflow.Individual, nodeWithOverflowQueue);

            IndividualLeftNode(overflow.BlockedServer, overflow.BlockedNode);
            // The node from which the overflow is coming from, might now accept also a new overflowing individual
            // Check this in the next iteration of this queue
            overflowHandling.Enqueue(overflow.BlockedNode);
            _state.Exit(overflow.Individual, overflow.BlockedNode, overflow.ArrivalTime, _time);
        }
    }

    private bool TryLeaveIndividual(Individual individual, SimulationNode origin, int arrivalTime, int server)
    {
        RoutingDecision routingDecision = individual.Cohort.Routing.RouteAfterService(_state!.InputNodesById[origin.Id], _state!.InputNodesById.Values, _random);
        switch (routingDecision)
        {
            case ExitSystem:
                _state!.Exit(individual, origin, arrivalTime, _time);
                return true;
            case SeekDestination seekDestination:
                // The individual tries to seek the chosen destination
                if (!_state.Nodes[seekDestination.Destination.Id].IsQueueFull)
                {
                    _state!.ExitToDestination(individual, origin, arrivalTime, _time, _state.Nodes[seekDestination.Destination.Id]);
                    IndividualArrives(individual, _state.Nodes[seekDestination.Destination.Id]);
                    return true;
                }
                // Queue at destination is full
                switch (seekDestination.QueueIsFullBehavior)
                {
                    case QueueIsFullBehavior.RejectIndividual:
                        _state!.Exit(individual, origin, arrivalTime, _time);
                        SimulationNode destination = _state.Nodes[seekDestination.Destination.Id];
                        _state!.AddArrival(individual, destination, _time);
                        _state!.Reject(individual, destination, _time, RejectionReason.QueueFull);
                        return true;
                    case QueueIsFullBehavior.WaitAndBlockCurrentServer:
                        Overflow overflow = new(individual, origin, server, arrivalTime);
                        _state.Nodes[seekDestination.Destination.Id].OverflowQueue.Enqueue(overflow);
                        return false;
                    default:
                        throw new NotImplementedEventException(seekDestination.QueueIsFullBehavior.ToString());

                }
            default:
                throw new NotImplementedEventException(routingDecision.GetType().Name);
        }
    }

    private void TryServerIndividual(Individual individual, SimulationNode destination, int arrivalTime)
    {
        NodeProperties nodeProperties = individual.Cohort.GetPropertiesById(destination.Id);
        bool canSelectServer = nodeProperties.ServerSelector.CanSelectServer(_state!.Nodes[destination.Id].ServingIndividuals, out int? selectedServer);
        if (!canSelectServer)
        {
            if (destination.IsQueueFull)
            {
                _state!.Reject(individual, destination, arrivalTime, RejectionReason.CannotSelectServer, _time);
                return;
            }
            destination.Queue.Enqueue((individual, arrivalTime));
            return;
        }
        if (selectedServer >= destination.ServerCount ||
            destination.ServingIndividuals[selectedServer!.Value] is not null)
        {
            throw new ImplausibleStateException($"Cannot select the server {selectedServer} for the Node {destination.Id}.");
        }
        TryStartService(individual, destination, nodeProperties.ServiceDurationSelector, arrivalTime, selectedServer.Value);
    }

    private bool TryStartService(Individual individual, SimulationNode node, DurationDistributionSelector serviceDurationSelector, int arrivalTime, int selectedServer)
    {
        node.ServingIndividuals[selectedServer] = individual;
        bool canCompleteService = serviceDurationSelector.TryGetNextTime(_time, _random, out int? serviceCompleted, false);
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
        _state!.EventQueue.Enqueue(completeServiceEvent, completeServiceEvent.Timestamp);
        _state!.StartService(individual, node, arrivalTime, _time, selectedServer);
        return true;
    }

    private void IndividualLeftNode(int server, SimulationNode origin)
    {
        while (!origin.IsQueueEmpty)
        {
            // Try to start the service of the next individual
            // If the individual is rejected, try the next individual
            // If the individual can get served, break this loop
            (Individual nextIndividual, int nextIndividualsArrivalTime) = origin.Queue.Dequeue();
            bool canStartService = TryStartService(nextIndividual, origin, nextIndividual.Cohort.GetPropertiesById(origin.Id).ServiceDurationSelector, nextIndividualsArrivalTime, server);
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
        foreach (SimulationNode node in _state.Nodes.Values)
        {
            node.Queue.Clear();
            node.OverflowQueue.Clear();
            node.SetServerCount(node.ServerCount);
        }
    }
}
