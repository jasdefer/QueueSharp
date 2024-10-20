namespace QueueSharp.Model.DurationDistribution;
internal class ConstantDuration : IDurationDistribution
{
    private readonly int _duration;

    internal ConstantDuration(int duration)
    {
        _duration = duration;
    }
    public int GetDuration()
    {
        return _duration;
    }
}