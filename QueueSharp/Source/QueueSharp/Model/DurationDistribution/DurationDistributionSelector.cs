using QueueSharp.Model.Exceptions;
using QueueSharp.StructureTypes;
using System.Collections.Immutable;

namespace QueueSharp.Model.DurationDistribution;

internal record ArrivalDistribution(DurationDistributionSelector DurationDistributionSelector, int ArrivalBatchSize = 1);
internal record DurationDistributionSelector
{
    private readonly ImmutableArray<(Interval, IDurationDistribution)> _durationDistributions;

    internal DurationDistributionSelector(IEnumerable<(Interval, IDurationDistribution)> durationDistributions)
    {
        if (durationDistributions.Any(x => durationDistributions.Any(y => x.Item1.Overlaps(y.Item1))))
        {
            throw new InvalidInputException($"Duration Distributions Overlap");
        }
        _durationDistributions = durationDistributions.OrderBy(x => x.Item1.Start)
            .ToImmutableArray();
    }

    internal int? GetDuration(int time)
    {
        for (int i = 0; i < _durationDistributions.Length; i++)
        {
            (Interval interval, IDurationDistribution distribution) = _durationDistributions[i];
            if (interval.IsInRange(time))
            {
                int duration = distribution.GetDuration();
                // ToDo: Handle fuzzyness at the end of an interval
                // If an interval is [0;10] and at the time 9 the duration 6 is generated, the end of the duration is way after the end of that interval
                // Durating the next interval a total different distribution is active
                return duration;
            }
        }

        // ToDo: Improve return type, if no duration can be generated
        return null;
    }
}
