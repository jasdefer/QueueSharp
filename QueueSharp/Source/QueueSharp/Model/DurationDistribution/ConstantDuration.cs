namespace QueueSharp.Model.DurationDistribution;
public class ConstantDuration : IDurationDistribution
{
    private readonly int _duration;

    public ConstantDuration(int duration)
    {
        _duration = duration;
    }
    public int GetDuration(Random? random)
    {
        return _duration;
    }
}