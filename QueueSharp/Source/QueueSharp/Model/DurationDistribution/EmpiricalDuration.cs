using System.Collections.Immutable;

namespace QueueSharp.Model.DurationDistribution;

/// <summary>
/// Generate a duration based on empirical data.
/// </summary>
public record EmpiricalDuration : IDurationDistribution
{
    private readonly ImmutableArray<int> _empiricalDurations;
    private readonly bool _randomizeOrder;
    private int _index = 0;

    /// <summary>
    /// 
    /// </summary>
    /// <param name="empiricalDurations">The list of duration, from which random values are selected.</param>
    /// <param name="randomizeOrder">True, if the order of values is random. False, if the values are selected based on their order.</param>
    public EmpiricalDuration(IEnumerable<int> empiricalDurations, bool randomizeOrder = true)
    {
        _empiricalDurations = empiricalDurations.ToImmutableArray();
        _randomizeOrder = randomizeOrder;
        _index = _empiricalDurations.Length;
    }

    /// <summary>
    /// Get the next duration value.
    /// </summary>
    public int GetDuration(Random? random = null)
    {
        random ??= new Random();
        if (_randomizeOrder)
        {
            return _empiricalDurations[random.Next(0, _empiricalDurations.Length)];
        }
        if (_index >= _empiricalDurations.Length - 1)
        {
            _index = 0;
        }
        else
        {
            _index++;
        }
        return _empiricalDurations[_index];
    }
}
