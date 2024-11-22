using QueueSharp.Model.Exceptions;
using QueueSharp.StructureTypes;
using System.Collections.Immutable;

namespace QueueSharp.Model.Helper;

/// <summary>
/// A general helper structure to represent a collection of values and intervals.
/// </summary>
/// <typeparam name="TValue"></typeparam>
public record IntervalDictionary<TValue>
{
    /// <summary>
    /// Stores the collection of intervals and the corresponding values.
    /// </summary>
    protected readonly ImmutableArray<(Interval, TValue)> _values;

    /// <summary>
    /// Create a new <see cref="IntervalDictionary{TValue}"/>. The Intervals cannot overlap.
    /// </summary>
    /// <param name="values"></param>
    /// <exception cref="InvalidInputException"></exception>
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

    /// <summary>
    /// Find the interval which bounds the given time and return the corresponding value.
    /// </summary>
    /// <param name="time">The time for which the value is requested.</param>
    /// <param name="index">The index of the interval value tuple, which bounds the given <paramref name="time"/>.</param>
    /// <param name="value">The requested value.</param>
    /// <returns>Returns true if an interval and the corresponding value can be found and false if no interval bounds the <paramref name="time"/>.</returns>
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
