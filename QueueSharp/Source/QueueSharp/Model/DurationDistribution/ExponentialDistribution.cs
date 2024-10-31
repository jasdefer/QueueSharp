using MathNet.Numerics.Distributions;

namespace QueueSharp.Model.DurationDistribution;
public class ExponentialDistribution : IDurationDistribution
{
    private readonly Random _random;
    private readonly double _rate;

    public ExponentialDistribution(double rate,
        int? randomSeed)
    {
        _random = randomSeed is null
            ? new Random()
            : new Random(randomSeed.Value);
        _rate = rate;
    }

    public int GetDuration()
    {
        int duration = (int)Math.Round(Exponential.Sample(_random, _rate));
        return duration;
    }
}
