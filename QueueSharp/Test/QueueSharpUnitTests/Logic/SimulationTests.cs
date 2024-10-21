using QueueSharp.Factories;
using QueueSharp.Logic;
using QueueSharp.Model.Components;
using QueueSharp.Model.Events;
using System.Collections.Immutable;

namespace QueueSharpUnitTests.Logic;
public class SimulationTests
{
    [Fact]
    public void SingleNode()
    {
        Cohort[] cohorts = CohortFactory.GetSingleNode("Node01", 2);
        Simulation simulation = new(cohorts);
        ImmutableArray<ActivityLog> result = simulation.Start();
        ImmutableArray<IEvent> eventLogs = result.OfType<EventLog>().Select(x => x.Event).ToImmutableArray();
        eventLogs.OfType<ArrivalEvent>().Should().HaveCount(100);
        eventLogs.OfType<CompleteServiceEvent>().Should().HaveCount(100);
        result.OfType<BaulkingLog>().Should().BeEmpty();
    }

    [Fact]
    public void ThreeNode()
    {
        Cohort[] cohorts = CohortFactory.GetEventEntrance();
        Simulation simulation = new(cohorts);
        ImmutableArray<ActivityLog> result = simulation.Start();
        ImmutableArray<IEvent> eventLogs = result.OfType<EventLog>().Select(x => x.Event).ToImmutableArray();
        eventLogs.OfType<ArrivalEvent>().Should().HaveCount(100);
        eventLogs.OfType<CompleteServiceEvent>().Should().HaveCount(100);
        result.OfType<BaulkingLog>().Should().BeEmpty();
    }
}
