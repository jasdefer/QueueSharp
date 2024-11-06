namespace QueueSharp.Model.DurationDistribution;
public class UniformDuration : IDurationDistribution
{
    private readonly int _min;
    private readonly int _max;

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

    public int GetDuration(Random? random)
    {
        random ??= new Random();
        return random.Next(_min, _max);
    }
}
