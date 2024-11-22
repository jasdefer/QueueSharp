using QueueSharp.Model.Helper;
using QueueSharp.StructureTypes;

namespace QueueSharp.Model.DurationDistribution;

/// <summary>
/// Manages a collection of duration distributions over specified intervals, enabling the selection
/// and calculation of arrival times based on these distributions. Each interval can have a different
/// distribution, providing flexibility for time-dependent duration management.
/// </summary>
/// <remarks>
/// This selector allows for flexible, interval-based duration calculations, particularly useful in 
/// event-driven systems where arrival and service times may vary over time.
/// </remarks>
public record DurationDistributionSelector : IntervalDictionary<IDurationDistribution>
{
    /// <summary>
    /// Represents a <see cref="DurationDistributionSelector"/> instance with no duration distributions.
    /// </summary>
    public readonly static DurationDistributionSelector None = new DurationDistributionSelector([]);
    private readonly double? _initialArrivalFraction;

    /// <summary>
    /// Initializes a new instance of the <see cref="DurationDistributionSelector"/> class with specified duration distributions.
    /// </summary>
    /// <param name="durationDistributions">A collection of tuples containing intervals and corresponding duration distributions.</param>
    /// <param name="initialArrivalFraction">
    /// Optional fraction to control the initial arrival time within the first interval.
    /// A value between 0 and 1, where null defaults to random generation.
    /// </param>
    public DurationDistributionSelector(IEnumerable<(Interval, IDurationDistribution)> durationDistributions,
        double? initialArrivalFraction = null)
        : base(durationDistributions)
    {
        _initialArrivalFraction = initialArrivalFraction;
    }

    /// <summary>
    /// Attempts to calculate the next arrival time based on the current time and interval distributions.
    /// </summary>
    /// <param name="random"></param>
    /// <param name="time">The current time to calculate the next arrival from.</param>
    /// <param name="arrival">
    /// When this method returns, contains the calculated arrival time if successful, or null if no valid 
    /// arrival time could be generated within the available intervals.
    /// </param>
    /// <param name="isInitialArrival">Indicates if this is the initial arrival, allowing for a fractional adjustment.</param>
    /// <returns>
    /// True if a valid arrival time was calculated; otherwise, false if no arrival could be scheduled within the intervals.
    /// </returns>
    /// <remarks>
    /// This method iterates through the intervals and calculates a duration based on the 
    /// configured duration distribution. It supports an initial arrival adjustment with a specified fraction, 
    /// and applies overflow handling if the interval ends before the calculated arrival.
    /// </remarks>
    internal bool TryGetNextTime(int time, Random random, out int? arrival, bool isInitialArrival = false)
    {
        arrival = 0;
        double fraction = isInitialArrival
            ? _initialArrivalFraction ?? random.NextDouble()
            : 1;

        for (int i = 0; i < _values.Length; i++)
        {
            (Interval interval, IDurationDistribution distribution) = _values[i];
            if (interval.End < time)
            {
                continue;
            }

            time = Math.Max(time, interval.Start);

            int currentDuration = (int)Math.Round(fraction * distribution.GetDuration(random));
            int end = time + currentDuration;
            if (end <= interval.End)
            {
                arrival = time + currentDuration;
                return true;
            }

            fraction = 1 - (interval.End - time) / (double)currentDuration;
        }

        arrival = null;
        return false;
    }
}
