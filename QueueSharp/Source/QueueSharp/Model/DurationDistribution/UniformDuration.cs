namespace QueueSharp.Model.DurationDistribution;
public class UniformDuration : IDurationDistribution
{
    private readonly Random _random;
    private readonly int _min;
    private readonly int _max;

    public UniformDuration(int min,
        int max,
        int? randomSeed)
    {
        _random = randomSeed is null
            ? new Random()
            : new Random(randomSeed.Value);
        if (min >= max)
        {
            throw new ArgumentOutOfRangeException(nameof(max));
        }
        _min = min;
        _max = max;
    }

    public int GetDuration()
    {
        return _random.Next(_min, _max);
    }
}
