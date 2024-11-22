namespace QueueSharp.Model.DurationDistribution;

/// <summary>
/// Computes a random uniformly distributed duration.
/// </summary>
public class UniformDuration : IDurationDistribution
{
    private readonly int _min;
    private readonly int _max;

    /// <summary>
    /// </summary>
    /// <param name="min">The minimum value.</param>
    /// <param name="max">The exclusive upper bound.</param>
    public UniformDuration(int min,
        int max)
    {
        if (min >= max)
        {
            throw new ArgumentOutOfRangeException(nameof(max));
        }
        _min = min;
        _max = max;
    }

    /// <summary>
    /// Get a new uniformly distributed duration.
    /// </summary>
    public int GetDuration(Random? random)
    {
        random ??= new Random();
        return random.Next(_min, _max);
    }
}
