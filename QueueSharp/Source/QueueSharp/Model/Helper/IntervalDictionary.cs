using QueueSharp.Model.Exceptions;
using QueueSharp.StructureTypes;
using System.Collections.Immutable;

namespace QueueSharp.Model.Helper;
public record IntervalDictionary<TValue>
{
    protected readonly ImmutableArray<(Interval, TValue)> _values;

    public IntervalDictionary(IEnumerable<(Interval, TValue)> values)
    {
        _values = values
            .OrderBy(x => x.Item1.Start)
            .ToImmutableArray();
        if (_values.Any(x => _values.Any(y => x.Item1 != y.Item1 && x.Item1.Overlaps(y.Item1))))
        {
            throw new InvalidInputException($"Duration Distributions Overlap");
        }
    }

    public bool TryGetAtTime(int time, out int? index, out TValue? value)
    {
        index = null;
        value = default;
        // ToDo: Implement Binary Search to improve performance
        for (int i = 0; i < _values.Length; i++)
        {
            (Interval currentInterval, TValue currentValue) = _values[i];

            if (currentInterval.IsInRange(time))
            {
                index = i;
                value = currentValue;
                return true;
            }
            if (currentInterval.Start > time)
            {
                return false;
            }
        }
        return false;
    }
}
