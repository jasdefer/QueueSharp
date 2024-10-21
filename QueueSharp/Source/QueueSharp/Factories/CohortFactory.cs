using QueueSharp.Model.Components;
using QueueSharp.Model.DurationDistribution;
using QueueSharp.Model.Routing;
using QueueSharp.Model.ServerSelector;
using QueueSharp.StructureTypes;
using System.Collections.Frozen;

namespace QueueSharp.Factories;
internal static class CohortFactory
{
    internal static Cohort[] GetSingleNode(string nodeId,
        int serverCount,
        IServerSelector? serverSelector = null)
    {
        Node[] nodes = [
            new Node(nodeId, serverCount: serverCount),
            ];
        IEnumerable<(Interval, IDurationDistribution)> arrivalDistribtionList = [
            IntervalForConstantDuration(start: 0, end: 1000, duration: 10)
            ];
        IEnumerable<(Interval, IDurationDistribution)> serviceDistributionList = [
            IntervalForConstantDuration(start: 0, end: 2000, duration: 30)
            ];
        DurationDistributionSelector arrivalDistributions = new(arrivalDistribtionList, 1);
        DurationDistributionSelector serviceDistributions = new(serviceDistributionList, 1);
        IRouting routing = new RandomRouteSelection(arcs: [], QueueIsFullBehavior.Baulk, 1);
        FrozenDictionary<Node, NodeProperties> propertiesByNode = new Dictionary<Node, NodeProperties>()
        {
            {nodes[0], new NodeProperties(arrivalDistributions, serviceDistributions, serverSelector??new FirstServerSelector()) }
        }.ToFrozenDictionary();
        Cohort[] cohorts = [
                new Cohort("Cohort01", propertiesByNode, routing,1)
            ];
        return cohorts;
    }

    internal static Cohort[] GetEventEntrance(IServerSelector? serverSelector = null)
    {
        Node[] nodes = [
            new Node("Ticket Gate", serverCount: 3),
            new Node("Bag Inspection", serverCount: 4),
            new Node("Person Metal Detector", serverCount: 2),
            ];
        DurationDistributionSelector arrivalPersonsWithoutATicket = new([
            IntervalForConstantDuration(start: 0, end: 1000, duration: 30),
            IntervalForConstantDuration(start: 1000, end: 2000, duration: 15),
            IntervalForConstantDuration(start: 2000, end: 3000, duration: 45),
            ], 1);
        DurationDistributionSelector arrivalPersonsWithATicketWithBag = new([
            IntervalForConstantDuration(start: 0, end: 1000, duration: 20),
            IntervalForConstantDuration(start: 1000, end: 2000, duration: 10),
            IntervalForConstantDuration(start: 2000, end: 3000, duration: 30),
            ], 1);
        DurationDistributionSelector arrivalPersonsWithATicketWithoutBag = new([
            IntervalForConstantDuration(start: 0, end: 1000, duration: 20),
            IntervalForConstantDuration(start: 1000, end: 2000, duration: 10),
            IntervalForConstantDuration(start: 2000, end: 3000, duration: 30),
            ], 1);
        DurationDistributionSelector ticketCheckpointService = new([
            IntervalForConstantDuration(start: 0, end: 5000, duration: 12)
            ], 1);
        DurationDistributionSelector bagInspection = new([
            IntervalForConstantDuration(start: 0, end: 5000, duration: 30)
            ], 1);
        DurationDistributionSelector metalDetectorServiceWithoutBag = new([
            IntervalForConstantDuration(start: 0, end: 5000, duration: 10)
            ]);
        DurationDistributionSelector metalDetectorServiceWithBag = new([
            IntervalForConstantDuration(start: 0, end: 5000, duration: 15)
            ], 1);

        IRouting nonTicketHolderRouting = new RandomRouteSelection(arcs: [], QueueIsFullBehavior.Baulk, 1);
        IRouting routingWithTicketAndBag = new RandomRouteSelection(arcs: GetArcs(nodes, 0, 1, 2), QueueIsFullBehavior.WaitAndBlockCurrentServer, 1);
        IRouting routingWithTicketWihtoutBag = new RandomRouteSelection(arcs: GetArcs(nodes, 0, 2), QueueIsFullBehavior.WaitAndBlockCurrentServer, 1);

        FrozenDictionary<Node, NodeProperties> propertiesByNodeNonTicketholders = new Dictionary<Node, NodeProperties>()
        {
            {nodes[0], new NodeProperties(arrivalPersonsWithoutATicket, ticketCheckpointService, serverSelector??new FirstServerSelector()) }
        }.ToFrozenDictionary();

        FrozenDictionary<Node, NodeProperties> propertiesByNodeTicketholdersWithBag = new Dictionary<Node, NodeProperties>()
        {
            {nodes[0], new NodeProperties(arrivalPersonsWithATicketWithBag, ticketCheckpointService, serverSelector??new FirstServerSelector()) },
            {nodes[1], new NodeProperties(DurationDistributionSelector.Empty, bagInspection, serverSelector??new FirstServerSelector()) },
            {nodes[2], new NodeProperties(DurationDistributionSelector.Empty, metalDetectorServiceWithBag, serverSelector??new FirstServerSelector()) },
        }.ToFrozenDictionary();

        FrozenDictionary<Node, NodeProperties> propertiesByNodeTicketholdersWithoutBag = new Dictionary<Node, NodeProperties>()
        {
            {nodes[0], new NodeProperties(arrivalPersonsWithATicketWithoutBag, ticketCheckpointService, serverSelector??new FirstServerSelector()) },
            {nodes[2], new NodeProperties(DurationDistributionSelector.Empty, metalDetectorServiceWithoutBag, serverSelector??new FirstServerSelector()) },
        }.ToFrozenDictionary();
        Cohort[] cohorts = [
                new Cohort("Without Ticket", propertiesByNodeNonTicketholders, nonTicketHolderRouting,1),
                new Cohort("With Ticket and bags", propertiesByNodeTicketholdersWithBag, routingWithTicketAndBag,1),
                new Cohort("With Ticket without bag", propertiesByNodeTicketholdersWithoutBag, routingWithTicketWihtoutBag,1),
            ];
        return cohorts;
    }

    private static (Interval, ConstantDuration) IntervalForConstantDuration(int start, int end, int duration)
    {
        return (new Interval(start, end), new ConstantDuration(duration));
    }

    private static IEnumerable<Arc> GetArcs(Node[] nodes, params int[] nodeIds)
    {
        for (int i = 0; i < nodeIds.Length - 1; i++)
        {
            int originId = nodeIds[i];
            int destinationId = nodeIds[i + 1];
            yield return new Arc(nodes[originId], nodes[destinationId]);
        }
    }
}
