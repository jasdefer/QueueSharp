using BenchmarkDotNet.Reports;
using BenchmarkDotNet.Running;
using QueueSharp.Logic;
using QueueSharp.Model.Components;
using QueueSharp.Model.DurationDistribution;
using QueueSharp.Model.Routing;
using QueueSharp.Model.ServerSelector;
using QueueSharp.StructureTypes;
using System.Collections.Frozen;
using System.Collections.Generic;
using System.Collections.Immutable;

//Summary[] summary = BenchmarkRunner.Run(typeof(Program).Assembly);
int ticksPerHour = 3600 * 1000;
int[] arrivalsPerHour = [32490, 45435, 13351, 8159, 2611, 274];
Interval[] intervals = new Interval[arrivalsPerHour.Length];
(Interval, IDurationDistribution)[] arrivals = new (Interval, IDurationDistribution)[arrivalsPerHour.Length];
for (int i = 0; i < intervals.Length; i++)
{
    Interval interval = new (ticksPerHour * i, ticksPerHour * (i + 1) - 1);
    IDurationDistribution durationDistribution = new ConstantDuration(ticksPerHour/ arrivalsPerHour[i]);
    arrivals[i] = new(interval, durationDistribution);
}

int headway = 35 * ticksPerHour / 60; // 10 minutes
int dwellDuration = 10 * ticksPerHour / 60; // 4 minutes
int trainCapacity = 12000;
IDurationDistribution boardingDistribution = new ConstantDuration(dwellDuration / trainCapacity);
int now = 0;
List<(Interval, IDurationDistribution)> serviceDurations = [];
while (now < intervals[^1].End + ticksPerHour * 80)
{
    Interval interval = new(now, now + dwellDuration);
    serviceDurations.Add((interval, boardingDistribution));
    now += dwellDuration;
    now += headway;
}

Node station = new Node("Station", 1);
NodeProperties stationProperties = new NodeProperties( ArrivalDistributionSelector: new DurationDistributionSelector(arrivals),
    ServiceDurationSelector: new DurationDistributionSelector(serviceDurations),
    ServerSelector: new FirstServerSelector(),
    ArrivalBatchSize: 1);

Dictionary<Node, NodeProperties> nodeProperties = new Dictionary<Node, NodeProperties>()
{
    {station, stationProperties }
};
IRouting routing = new RandomRouteSelection(arcs: [], null);
Cohort cohort = new("Passengers", nodeProperties.ToFrozenDictionary(), routing);

Simulation simulation = new([cohort], randomSeed: 1);
ImmutableArray<NodeVisitRecord> result = simulation.Start().ToImmutableArray();
NodeVisitRecord[] jo = result.Where(x => x is not NodeServiceRecord).ToArray();
SimulationReport report = SimulationAnalysis.GetSimulationReport(result);
File.WriteAllText("queue.csv", SimulationAnalysis.ToCsv(result));
Console.WriteLine($"Completed");