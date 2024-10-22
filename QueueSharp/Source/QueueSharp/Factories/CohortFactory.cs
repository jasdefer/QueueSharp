using QueueSharp.Model.Components;
using QueueSharp.Model.DurationDistribution;
using QueueSharp.Model.Routing;
using QueueSharp.Model.ServerSelector;
using QueueSharp.StructureTypes;
using System.Collections.Frozen;

namespace QueueSharp.Factories;
public static class CohortFactory
{
    public static Cohort[] GetSingleNode(string nodeId,
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

    public static Cohort[] GetEventEntrance(IServerSelector? serverSelector = null,
        double arrivalScaleFactor = 1,
        double arrivalInterfalFactor = 1)
    {
        int[] times = [0,
            (int)(1000 * arrivalInterfalFactor),
            (int)(2000 * arrivalInterfalFactor),
            (int)(3000 * arrivalInterfalFactor)];
        Node[] nodes = [
            new Node("Ticket Gate", serverCount: 3),
            new Node("Bag Inspection", serverCount: 4),
            new Node("Person Metal Detector", serverCount: 2),
            ];
        DurationDistributionSelector arrivalPersonsWithoutATicket = new([
            IntervalForConstantDuration(start: times[0], end: times[1], duration: (int)(30 * arrivalScaleFactor)),
            IntervalForConstantDuration(start: times[1], end: times[2], duration: (int)(15 * arrivalScaleFactor)),
            IntervalForConstantDuration(start: times[2], end: times[3], duration: (int)(45 * arrivalScaleFactor)),
            ], 1);
        DurationDistributionSelector arrivalPersonsWithATicketWithBag = new([
            IntervalForConstantDuration(start: times[0], end: times[1],duration: (int)(20 * arrivalScaleFactor)),
            IntervalForConstantDuration(start: times[1], end: times[2], duration: (int)(10 * arrivalScaleFactor)),
            IntervalForConstantDuration(start: times[2], end: times[3], duration: (int)(30 * arrivalScaleFactor)),
            ], 1);
        DurationDistributionSelector arrivalPersonsWithATicketWithoutBag = new([
            IntervalForConstantDuration(start: times[0], end: times[1], duration: (int)(20 * arrivalScaleFactor)),
            IntervalForConstantDuration(start: times[1], end: times[2], duration: (int)(10 * arrivalScaleFactor)),
            IntervalForConstantDuration(start: times[2], end: times[3], duration: (int)(30 * arrivalScaleFactor)),
            ], 1);
        DurationDistributionSelector ticketCheckpointService = new([
            IntervalForConstantDuration(start: 0, end: times[^1]*3, duration: 12)
            ], 1);
        DurationDistributionSelector bagInspection = new([
            IntervalForConstantDuration(start: 0, times[^1]*3, duration: 30)
            ], 1);
        DurationDistributionSelector metalDetectorServiceWithoutBag = new([
            IntervalForConstantDuration(start: 0, times[^1]*3, duration: 10)
            ]);
        DurationDistributionSelector metalDetectorServiceWithBag = new([
            IntervalForConstantDuration(start: 0, times[^1]*3, duration: 15)
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
