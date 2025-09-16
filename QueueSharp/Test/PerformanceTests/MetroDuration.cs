using QueueSharp.Model.DurationDistribution;
using QueueSharp.StructureTypes;
using System.Collections.Immutable;

namespace PerformanceTests;
public class MetroDuration : IDurationDistribution
{
    private readonly ImmutableArray<Interval> _serviceIntervals;
    public MetroDuration(IDurationDistribution serviceDuration, IEnumerable<Interval> serviceIntervals)
    {
        _serviceIntervals = serviceIntervals.ToImmutableArray();
    }
    public int GetDuration(Random? random)
    {
        throw new NotImplementedException();
    }
}
