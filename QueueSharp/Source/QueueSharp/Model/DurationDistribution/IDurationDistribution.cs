namespace QueueSharp.Model.DurationDistribution;

/// <summary>
/// Defines an interface for generating a duration based on a specific distribution strategy.
/// Implementations provide logic to determine the duration for various events or processes in a system.
/// </summary>
public interface IDurationDistribution
{
    /// <summary>
    /// Generates a duration based on the configured distribution.
    /// </summary>
    /// <returns>An integer representing the duration.</returns>
    int GetDuration(Random? random);
}

