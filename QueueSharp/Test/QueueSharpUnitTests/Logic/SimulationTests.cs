using QueueSharp.Factories;
using QueueSharp.Logic;
using QueueSharp.Logic.Validation;
using QueueSharp.Model.Components;
using System.Collections.Immutable;

namespace QueueSharpUnitTests.Logic;
public class SimulationTests
{
    [Fact]
    public void SingleNode()
    {
        Cohort[] cohorts = CohortFactory.GetSingleNode("Node01", 2);
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
}
