using MathNet.Numerics.Distributions;

namespace QueueSharp.Model.DurationDistribution;

/// <summary>
/// Generate a normal distributed duration.
/// </summary>
public class NormalDistributedDuration : IDurationDistribution
{
    /// <summary>
    /// The mean parameter of the normal distribution
    /// </summary>
    private readonly double _mean;

    /// <summary>
    /// The variance parameter of the normal distribution
    /// </summary>
    private readonly double _variance;

    /// <summary>
    /// Values are capped at this minimum value. If null, no minimum is enforced.
    /// </summary>
    private readonly int? _min;

    /// <summary>
    /// Values are capped at this maximum value. If null, no maximum is enforced.
    /// </summary>
    private readonly int? _max;

    /// <summary>
    /// Constructor to initialize the normal distributed duration parameters.
    /// </summary>
    public NormalDistributedDuration(double mean, double variance, int? min = null, int? max = null)
    {
        _mean = mean;
        _variance = variance;
        _min = min;
        _max = max;
    }

    /// <summary>
    /// Generate a normal distributed duration.
    /// </summary>
    public int GetDuration(Random? random)
    {
        random ??= new Random();
        int duration = (int)Math.Round(Normal.Sample(random, _mean, _variance));
        if (_min.HasValue && duration < _min.Value)
        {
            duration = _min.Value;
        }
        if (_max.HasValue && duration > _max.Value)
        {
            duration = _max.Value;
        }
        return duration;
    }
}
