using QueueSharp.StructureTypes;

namespace QueueSharp.Model.DurationDistribution;
public interface IDurationDistribution
{
    int GetDuration();
}

public static class DurationDistributionHelper
{
    public static DurationDistributionSelector ToSelector(this IDurationDistribution durationDistribution, 
        int start,
        int end, 
        int? randomSeed = null)
    {
        return new DurationDistributionSelector([(new Interval(start, end), durationDistribution)], randomSeed);
    }
}

