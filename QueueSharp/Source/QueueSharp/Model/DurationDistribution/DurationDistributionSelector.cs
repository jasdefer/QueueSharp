﻿using QueueSharp.Model.Helper;
using QueueSharp.StructureTypes;

namespace QueueSharp.Model.DurationDistribution;

internal record DurationDistributionSelector : IntervalDictionary<IDurationDistribution>
{
    private readonly Random _random;

    internal DurationDistributionSelector(IEnumerable<(Interval, IDurationDistribution)> durationDistributions, int? randomSeed = null)
        : base(durationDistributions)
    {
        _random = randomSeed is null
            ? new Random()
            : new Random(randomSeed.Value);
    }

    internal bool TryGetArrivalTime(int time, out int? arrival, bool isInitialArrival = false)
    {
        arrival = 0;
        double fraction = isInitialArrival
            ? _random.NextDouble()
            : 1;

        for (int i = 0; i < _values.Length; i++)
        {
            (Interval interval, IDurationDistribution distribution) = _values[i];
            if (interval.End < time)
            {
                continue;
            }

            time = Math.Max(time, interval.Start);

            int currentDuration = (int)Math.Round(fraction * distribution.GetDuration());
            int end = time + currentDuration;
            if (end <= interval.End)
            {
                arrival = time + currentDuration;
                return true;
            }

            fraction = 1 - (interval.End - time) / (double)currentDuration;
        }

        arrival = null;
        return false;
    }
}