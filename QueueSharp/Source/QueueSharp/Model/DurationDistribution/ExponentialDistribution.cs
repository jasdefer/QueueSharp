using MathNet.Numerics.Distributions;

namespace QueueSharp.Model.DurationDistribution;

/// <summary>
/// Generate an exponential distributed duration.
/// </summary>
public class ExponentialDistribution : IDurationDistribution
{
    private readonly double _rate;

    /// <summary>
    /// 
    /// </summary>
    /// <param name="rate">The rate of the exponential distribution.</param>
    public ExponentialDistribution(double rate)
    {
        _rate = rate;
    }

    /// <summary>
    /// Generate an exponential distributed duration.
    /// </summary>
    public int GetDuration(Random? random)
    {
        random ??= new Random();
        int duration = (int)Math.Round(Exponential.Sample(random, _rate));
        return duration;
    }
}
