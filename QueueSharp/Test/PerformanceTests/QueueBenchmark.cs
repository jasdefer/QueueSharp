using BenchmarkDotNet.Attributes;
using QueueSharp.Factories;
using QueueSharp.Logic;
using QueueSharp.Model.Components;

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

    //[Benchmark]
    //public int RunSmall()
    //{
    //    IEnumerable<NodeVisitRecord> result = _simulationSmall!.Start();
    //    return result.Count();
    //}

    //[Benchmark]
    //public int LongIntervals()
    //{
    //    IEnumerable<NodeVisitRecord> result = _simulationLongIntervals!.Start();
    //    return result.Count();
    //}

    [Benchmark]
    public int FrequentArrivals()
    {
        IEnumerable<NodeVisitRecord> result = _simulationFrequentArrival!.Start();
        return result.Count();
    }

    //[Benchmark]
    //public int LongIntervalsAndFrequentArrivals()
    //{
    //    IEnumerable<NodeVisitRecord> result = _simulationLongIntervalsAndFrequentArrivals!.Start();
    //    return result.Count();
    //}
}
