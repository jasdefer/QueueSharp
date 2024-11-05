using MathNet.Numerics.Distributions;

namespace QueueSharp.Model.DurationDistribution;
public class ExponentialDistribution : IDurationDistribution
{
    private readonly double _rate;
    public ExponentialDistribution(double rate)
    {
        _rate = rate;
    }

    public int GetDuration(Random? random)
    {
        random ??= new Random();
        int duration = (int)Math.Round(Exponential.Sample(random, _rate));
        return duration;
    }
}
