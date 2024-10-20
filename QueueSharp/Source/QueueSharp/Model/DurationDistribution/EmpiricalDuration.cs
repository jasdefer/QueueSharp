using System.Collections.Immutable;

namespace QueueSharp.Model.DurationDistribution;
internal record EmpiricalDuration : IDurationDistribution
{
    private readonly ImmutableArray<int> _empiricalDurations;
    private readonly Random _random;

    public EmpiricalDuration(IEnumerable<int> empiricalDurations, int? randomSeed)
    {
        _empiricalDurations = empiricalDurations.ToImmutableArray();
        _random = randomSeed is null
            ? new Random()
            : new Random(randomSeed.Value);

    }
    public int GetDuration()
    {
        return _empiricalDurations[_random.Next(0, _empiricalDurations.Length)];
    }
}
