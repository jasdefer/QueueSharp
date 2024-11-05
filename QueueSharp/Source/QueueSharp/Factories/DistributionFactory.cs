using QueueSharp.Model.DurationDistribution;
using QueueSharp.StructureTypes;

namespace QueueSharp.Factories;
public static class DistributionFactory
{
    #region Constant Duration

    /// <summary>
    /// Creates a uniform duration distribution selector within a specified start and end time interval.
    /// </summary>
    /// <param name="start">The start time of the interval.</param>
    /// <param name="end">The end time of the interval.</param>
    /// <param name="min">The minimum duration for the uniform distribution.</param>
    /// <param name="max">The maximum duration for the uniform distribution.</param>
    /// <param name="randomSeed">Optional seed for random generation, ensuring reproducibility.</param>
    /// <param name="arrivalFraction">Optional fraction to adjust initial arrival time for the first generated interval.</param>
    /// <returns>A <see cref="DurationDistributionSelector"/> configured with a uniform distribution for the specified interval.</returns>
    public static DurationDistributionSelector CreateUniform(int start, int end, int min, int max, int? randomSeed = null, double? arrivalFraction = null)
    {
        return CreateUniform(new Interval(start, end), min, max, randomSeed, arrivalFraction);
    }

    /// <summary>
    /// Creates a uniform duration distribution selector for a given interval.
    /// </summary>
    /// <param name="interval">The interval for the duration distribution.</param>
    /// <param name="min">The minimum duration for the uniform distribution.</param>
    /// <param name="max">The maximum duration for the uniform distribution.</param>
    /// <param name="randomSeed">Optional seed for random generation, ensuring reproducibility.</param>
    /// <param name="arrivalFraction">Optional fraction to adjust initial arrival time for the first generated interval.</param>
    /// <returns>A <see cref="DurationDistributionSelector"/> configured with a uniform distribution for the specified interval.</returns>
    public static DurationDistributionSelector CreateUniform(Interval interval, int min, int max, int? randomSeed = null, double? arrivalFraction = null)
    {
        return CreateUniform([(interval, min, max)], randomSeed, arrivalFraction);
    }

    /// <summary>
    /// Creates a uniform duration distribution selector with multiple interval parameters.
    /// </summary>
    /// <param name="parameters">A collection of parameters where each tuple represents an interval with a minimum and maximum duration for the uniform distribution.</param>
    /// <param name="randomSeed">Optional seed for random generation, ensuring reproducibility.</param>
    /// <param name="arrivalFraction">Optional fraction to adjust initial arrival time for the first generated interval.</param>
    /// <returns>A <see cref="DurationDistributionSelector"/> configured with multiple uniform distributions over specified intervals.</returns>
    public static DurationDistributionSelector CreateUniform(IEnumerable<(Interval, int, int)> parameters, int? randomSeed = null, double? arrivalFraction = null)
    {
        IEnumerable<(Interval, IDurationDistribution)> durationDistributions = parameters
            .Select(x => (x.Item1, (IDurationDistribution)new UniformDuration(min: x.Item2, max: x.Item3, randomSeed: randomSeed is null ? randomSeed : ++randomSeed)));
        return new DurationDistributionSelector(durationDistributions: durationDistributions, randomSeed: randomSeed is null ? randomSeed : ++randomSeed, initialArrivalFraction: arrivalFraction);
    }
    #endregion

    #region Constant Duration

    /// <summary>
    /// Creates a constant duration distribution selector within a specified start and end time interval.
    /// </summary>
    /// <param name="start">The start time of the interval.</param>
    /// <param name="end">The end time of the interval.</param>
    /// <param name="duration">The fixed duration value for the constant distribution.</param>
    /// <param name="randomSeed">Optional seed for random generation, ensuring reproducibility.</param>
    /// <param name="arrivalFraction">Optional fraction to adjust initial arrival time for the first generated interval.</param>
    /// <returns>A <see cref="DurationDistributionSelector"/> configured with a constant duration distribution for the specified interval.</returns>
    public static DurationDistributionSelector CreateConstant(int start, int end, int duration, int? randomSeed = null, double? arrivalFraction = null)
    {
        return CreateConstant(new Interval(start, end), duration, randomSeed, arrivalFraction);
    }

    /// <summary>
    /// Creates a constant duration distribution selector for a given interval.
    /// </summary>
    /// <param name="interval">The interval for the duration distribution.</param>
    /// <param name="duration">The fixed duration value for the constant distribution.</param>
    /// <param name="randomSeed">Optional seed for random generation, ensuring reproducibility.</param>
    /// <param name="arrivalFraction">Optional fraction to adjust initial arrival time for the first generated interval.</param>
    /// <returns>A <see cref="DurationDistributionSelector"/> configured with a constant duration distribution for the specified interval.</returns>
    public static DurationDistributionSelector CreateConstant(Interval interval, int duration, int? randomSeed = null, double? arrivalFraction = null)
    {
        return CreateConstant([(interval, duration)], randomSeed, arrivalFraction);
    }

    /// <summary>
    /// Creates a constant duration distribution selector with multiple interval parameters.
    /// </summary>
    /// <param name="parameters">A collection of parameters where each tuple represents an interval and a fixed duration for the constant distribution.</param>
    /// <param name="randomSeed">Optional seed for random generation, ensuring reproducibility.</param>
    /// <param name="arrivalFraction">Optional fraction to adjust initial arrival time for the first generated interval.</param>
    /// <returns>A <see cref="DurationDistributionSelector"/> configured with multiple constant distributions over specified intervals.</returns>
    public static DurationDistributionSelector CreateConstant(IEnumerable<(Interval, int)> parameters, int? randomSeed = null, double? arrivalFraction = null)
    {
        IEnumerable<(Interval, IDurationDistribution)> durationDistributions = parameters
            .Select(x => (x.Item1, (IDurationDistribution)new ConstantDuration(duration: x.Item2)));
        return new DurationDistributionSelector(durationDistributions, randomSeed, arrivalFraction);
    }
    #endregion

    #region Exponential

    /// <summary>
    /// Creates an exponential duration distribution selector within a specified start and end time interval.
    /// </summary>
    /// <param name="start">The start time of the interval.</param>
    /// <param name="end">The end time of the interval.</param>
    /// <param name="rate">The rate parameter for the exponential distribution.</param>
    /// <param name="randomSeed">Optional seed for random generation, ensuring reproducibility.</param>
    /// <param name="arrivalFraction">Optional fraction to adjust initial arrival time for the first generated interval.</param>
    /// <returns>A <see cref="DurationDistributionSelector"/> configured with an exponential duration distribution for the specified interval.</returns>
    public static DurationDistributionSelector CreateExponential(int start, int end, double rate, int? randomSeed = null, double? arrivalFraction = null)
    {
        return CreateExponential(new Interval(start, end), rate, randomSeed, arrivalFraction);
    }

    /// <summary>
    /// Creates an exponential duration distribution selector for a given interval.
    /// </summary>
    /// <param name="interval">The interval for the duration distribution.</param>
    /// <param name="rate">The rate parameter for the exponential distribution.</param>
    /// <param name="randomSeed">Optional seed for random generation, ensuring reproducibility.</param>
    /// <param name="arrivalFraction">Optional fraction to adjust initial arrival time for the first generated interval.</param>
    /// <returns>A <see cref="DurationDistributionSelector"/> configured with an exponential duration distribution for the specified interval.</returns>
    public static DurationDistributionSelector CreateExponential(Interval interval, double rate, int? randomSeed = null, double? arrivalFraction = null)
    {
        return CreateExponential([(interval, rate)], randomSeed, arrivalFraction);
    }

    /// <summary>
    /// Creates an exponential duration distribution selector with multiple interval parameters.
    /// </summary>
    /// <param name="parameters">A collection of parameters where each tuple represents an interval and a rate for the exponential distribution.</param>
    /// <param name="randomSeed">Optional seed for random generation, ensuring reproducibility.</param>
    /// <param name="arrivalFraction">Optional fraction to adjust initial arrival time for the first generated interval.</param>
    /// <returns>A <see cref="DurationDistributionSelector"/> configured with multiple exponential distributions over specified intervals.</returns>
    public static DurationDistributionSelector CreateExponential(IEnumerable<(Interval, double)> parameters, int? randomSeed = null, double? arrivalFraction = null)
    {
        IEnumerable<(Interval, IDurationDistribution)> durationDistributions = parameters
            .Select(x => (x.Item1, (IDurationDistribution)new ExponentialDistribution(rate: x.Item2, randomSeed: randomSeed is null ? randomSeed : ++randomSeed)));
        return new DurationDistributionSelector(durationDistributions, randomSeed is null ? randomSeed : ++randomSeed, arrivalFraction);
    }
    #endregion
}
