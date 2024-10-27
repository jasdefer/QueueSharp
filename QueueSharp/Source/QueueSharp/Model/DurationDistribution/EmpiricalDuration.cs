using System.Collections.Immutable;

namespace QueueSharp.Model.DurationDistribution;
internal record EmpiricalDuration : IDurationDistribution
{
    private readonly ImmutableArray<int> _empiricalDurations;
    private readonly Random _random;
    private readonly bool _randomizeOrder;
    private int _index = 0;

    public EmpiricalDuration(IEnumerable<int> empiricalDurations, bool randomizeOrder = true, int? randomSeed = null)
    {
        _empiricalDurations = empiricalDurations.ToImmutableArray();
        _random = randomSeed is null
            ? new Random()
            : new Random(randomSeed.Value);
        _randomizeOrder = randomizeOrder;
        _index = _empiricalDurations.Length;
    }
    public int GetDuration()
    {
        if (_randomizeOrder)
        {
            return _empiricalDurations[_random.Next(0, _empiricalDurations.Length)];
        }
        if(_index >= _empiricalDurations.Length - 1)
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
