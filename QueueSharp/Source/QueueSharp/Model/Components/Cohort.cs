using QueueSharp.Model.DurationDistribution;

namespace QueueSharp.Model.Components;
internal record Cohort
{
    internal Cohort(IDictionary<Node, DurationDistributionSelector> arrivalDistributionPerNode)
    {

    }
}