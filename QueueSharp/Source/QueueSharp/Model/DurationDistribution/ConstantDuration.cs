namespace QueueSharp.Model.DurationDistribution;

/// <summary>
/// Generates a constant duration every time.
/// </summary>
public class ConstantDuration : IDurationDistribution
{
    private readonly int _duration;

    /// <summary>
    /// 
    /// </summary>
    /// <param name="duration">The duration returned.</param>
    public ConstantDuration(int duration)
    {
        _duration = duration;
    }

    /// <summary>
    /// Returns the initialized duration.
    /// </summary>
    /// <param name="random"></param>
    /// <returns></returns>
    public int GetDuration(Random? random)
    {
        return _duration;
    }
}