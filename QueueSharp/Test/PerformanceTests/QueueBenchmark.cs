using BenchmarkDotNet.Attributes;
using QueueSharp.Factories;
using QueueSharp.Logic;
using QueueSharp.Model.Components;
using System.Collections.Immutable;

namespace PerformanceTests;

[MemoryDiagnoser]
public class QueueBenchmark
{
    private Simulation? _simulationSmall;
    private Simulation? _simulationLongIntervals;
    private Simulation? _simulationFrequentArrival;
    private Simulation? _simulationLongIntervalsAndFrequentArrivals;

    [GlobalSetup]
    public void Setup()
    {
        Cohort[] cohorts = CohortFactory.GetEventEntrance();
        _simulationSmall = new(cohorts);
        cohorts = CohortFactory.GetEventEntrance(null, 1, 100);
        _simulationLongIntervals = new(cohorts);
        cohorts = CohortFactory.GetEventEntrance(null, 0.1, 1);
        _simulationFrequentArrival = new(cohorts);
        cohorts = CohortFactory.GetEventEntrance(null, 0.1, 100);
        _simulationLongIntervalsAndFrequentArrivals = new(cohorts);
    }

    [Benchmark]
    public int RunSmall()
    {
        ImmutableArray<ActivityLog> result = _simulationSmall!.Start();
        return result.Length;
    }

    [Benchmark]
    public int LongIntervals()
    {
        ImmutableArray<ActivityLog> result = _simulationLongIntervals!.Start();
        return result.Length;
    }

    [Benchmark]
    public int FrequentArrivals()
    {
        ImmutableArray<ActivityLog> result = _simulationFrequentArrival!.Start();
        return result.Length;
    }

    [Benchmark]
    public int LongIntervalsAndFrequentArrivals()
    {
        ImmutableArray<ActivityLog> result = _simulationLongIntervalsAndFrequentArrivals!.Start();
        return result.Length;
    }
}
