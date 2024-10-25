using QueueSharp.Factories;
using QueueSharp.Logic;
using QueueSharp.Logic.Validation;
using QueueSharp.Model.Components;
using QueueSharp.Model.DurationDistribution;
using System.Collections.Frozen;
using System.Collections.Immutable;

namespace QueueSharpUnitTests.Logic;
public class SimulationTests
{
    [Fact]
    public void SingleNode()
    {
        Cohort[] cohorts = CohortFactory.GetSingleNode("Node01",
            serverCount: 2,
            arrivalDistribution: new ConstantDuration(10),
            serviceDistribution: new ConstantDuration(30),
            arrivalEnd: 1000,
            serviceEnd: 2000);
        Simulation simulation = new(cohorts);
        ImmutableArray<NodeVisitRecord> result = simulation.Start().ToImmutableArray();
        ImmutableArray<NodeServiceRecord> nodeServiceRecords = result.OfType<NodeServiceRecord>().ToImmutableArray();
        nodeServiceRecords.Should().HaveCount(100);
        nodeServiceRecords.Should().AllSatisfy(x => x.ServiceDuration.Should().Be(30));
        nodeServiceRecords.Should().AllSatisfy(x => x.Destination.Should().BeNull());
        for (int i = 1; i < nodeServiceRecords.Length; i++)
        {
            nodeServiceRecords[i].ArrivalTime.Should().Be(nodeServiceRecords[i - 1].ArrivalTime + 10);
            nodeServiceRecords[i].QueueSizeAtArrival.Should().BeGreaterThanOrEqualTo(nodeServiceRecords[i - 1].QueueSizeAtArrival);
        }
        NodeVisitRecordsValidation.Validate(result);
    }

    [Fact]
    public void ThreeNode()
    {
        Cohort[] cohorts = CohortFactory.GetEventEntrance(null, 0.1, 1);
        Simulation simulation = new(cohorts);
        ImmutableArray<NodeVisitRecord> result = simulation.Start().ToImmutableArray();
        NodeVisitRecordsValidation.Validate(result);
    }

    [Fact]
    public void CiwComparison()
    {
        SimulationReport[] reports = new SimulationReport[5000];
        for (int i = 0; i < reports.Length; i++)
        {
            Cohort[] cohorts = CohortFactory.GetSingleNode("Node01",
            serverCount: 3,
            new ExponentialDistribution(rate: 0.2, randomSeed: i),
            new ExponentialDistribution(rate: 0.1, randomSeed: i),
            arrivalEnd: 2000,
            serviceEnd: 2000);
            SimulationSettings simulationSettings = new(1440);
            Simulation simulation = new(cohorts, simulationSettings);
            IEnumerable<NodeVisitRecord> nodeVisitRecords = simulation.Start();
            reports[i] = SimulationAnalysis.GetSimulationReport(nodeVisitRecords);
        }

        FrozenDictionary<string, SimulationAggregationNodeReport> mergedReport = SimulationAnalysis.Merge(reports);
        mergedReport.Should().HaveCount(1);
        mergedReport.Single().Value.WaitingTimeMetrics.Mean.Mean.Should().BeApproximately(5, 3);
    }
}
