using BenchmarkDotNet.Attributes;
using QueueSharp.Factories;
using QueueSharp.Logic;
using QueueSharp.Model.Components;
using QueueSharp.Model.DurationDistribution;
using QueueSharp.Model.Routing;
using QueueSharp.Model.ServerSelector;
using System.Collections.Frozen;
using System.Collections.Immutable;

namespace PerformanceTests;

[MemoryDiagnoser]
public class QueueBenchmark
{
    [Params(20000, 200000, 2000000)]
    public int SimulationRuntime { get; set; }

    [Benchmark]
    public int RunThreeNodeExampleFromCiw()
    {
        Node coldFood = new Node("Cold Food", 1);
        Node hotFood = new Node("Hot Food", 2);
        Node till = new Node("Till", 2);

        WeightedArc coldToHot = new WeightedArc(coldFood, hotFood, 0.3);
        WeightedArc coldToTill = new WeightedArc(coldFood, till, 0.7);
        WeightedArc hotToTill = new WeightedArc(hotFood, till, 1);

        IRouting routing = new RandomRouteSelection([coldToHot, coldToTill, hotToTill], null);

        IServerSelector serverSelector = new FirstServerSelector();
        Dictionary<Node, NodeProperties> propertiesByNode = new Dictionary<Node, NodeProperties>
        {
            {
                coldFood, new NodeProperties(DistributionFactory.CreateExponential(start: 0, end: SimulationRuntime * 2, 0.003),
                    DistributionFactory.CreateExponential(start: 0, end: SimulationRuntime * 2, 0.01),
                    serverSelector)
            },
            {
                hotFood, new NodeProperties(DistributionFactory.CreateExponential(start: 0, end: SimulationRuntime * 2, 0.002),
                    DistributionFactory.CreateExponential(start: 0, end: SimulationRuntime * 2, 0.004),
                    serverSelector)
            },
            {
                till, new NodeProperties(DurationDistributionSelector.None,
                    DistributionFactory.CreateExponential(start: 0, end: SimulationRuntime * 2, 0.005),
                    serverSelector)
            }
        };

        Cohort[] cohorts = [
            new Cohort("Cohort01", propertiesByNode.ToFrozenDictionary(), routing)
            ];
        SimulationSettings simulationSettings = new(SimulationRuntime);
        Simulation simulation = new Simulation(cohorts, simulationSettings);
        ImmutableArray<NodeVisitRecord> result = simulation.Start().ToImmutableArray();
        return result.Length;
    }
}
