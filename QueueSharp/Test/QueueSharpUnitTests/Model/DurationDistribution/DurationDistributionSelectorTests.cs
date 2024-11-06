using QueueSharp.Model.DurationDistribution;
using QueueSharp.StructureTypes;

namespace QueueSharpUnitTests.Model.DurationDistribution;
public class DurationDistributionSelectorTests
{
    private static readonly Random _random = new Random(1);
    private static readonly List<(Interval, IDurationDistribution)> _distributions = [
                (new Interval(10, 20), new ConstantDuration(10)),
                (new Interval(50, 60), new ConstantDuration(20)),
                (new Interval(100, 130), new ConstantDuration(40)),
            ];
    private static readonly DurationDistributionSelector _selector = new(_distributions, 1);

    [Theory]
    [InlineData(0)]
    [InlineData(5)]
    [InlineData(10)]
    public void TryGetDuration_Between_0_And_10(int time)
    {
        bool canGet = _selector.TryGetNextTime(time, _random, out int? arrival, false);
        canGet.Should().BeTrue();
        arrival.Should().Be(20);
    }

    [Fact]
    public void TryGetDuration_At_15()
    {
        bool canGet = _selector.TryGetNextTime(15, _random, out int? arrival, false);
        canGet.Should().BeTrue();
        arrival.Should().Be(60);
    }

    [Theory]
    [InlineData(20)]
    [InlineData(30)]
    [InlineData(50)]
    public void TryGetDuration_Between_20_And_50(int time)
    {
        bool canGet = _selector.TryGetNextTime(time, _random, out int? arrival, false);
        canGet.Should().BeTrue();
        arrival.Should().Be(120);
    }

    [Fact]
    public void TryGetDuration_At_55()
    {
        bool canGet = _selector.TryGetNextTime(55, _random, out int? arrival, false);
        canGet.Should().BeTrue();
        arrival.Should().Be(130);
    }

    [Theory]
    [InlineData(60)]
    [InlineData(80)]
    [InlineData(100)]
    [InlineData(110)]
    [InlineData(130)]
    [InlineData(140)]
    public void TryGetDuration_Between_60_And_140(int time)
    {
        bool canGet = _selector.TryGetNextTime(time, _random, out int? arrival, false);
        canGet.Should().BeFalse();
    }

    [Fact]
    public void WithoutInitialArrival_And_One_Interval()
    {
        List<(Interval, IDurationDistribution)> distributions = [
                (new Interval(10, 20), new ConstantDuration(10))
            ];
        DurationDistributionSelector selector = new(distributions);

        double averageArrival = 0;
        int numberOfSuccess = 0;
        int numberOfIterations = 1000;
        for (int i = 0; i < numberOfIterations; i++)
        {
            bool canGet = selector.TryGetNextTime(15, _random, out int? arrival, true);
            if (canGet)
            {
                numberOfSuccess++;
                arrival.Should().NotBeNull();
                averageArrival += arrival!.Value;
            }
        }

        averageArrival /= numberOfSuccess;
        double fractionOfSuccess = numberOfSuccess / (double)numberOfIterations;
        fractionOfSuccess.Should().BeApproximately(0.5, 0.1);
        averageArrival.Should().BeApproximately(17.5, 0.3);
    }

    [Fact]
    public void JumpOverTwoIntervals()
    {
        List<(Interval, IDurationDistribution)> distributions = [
                (new Interval(10, 20), new ConstantDuration(20)), // 5 time unites gone (25% gone, 75% remaining)
                (new Interval(30, 40), new ConstantDuration(40)), // 10/30 (30= 75%*40) time units gone (33% gone, 67% remaining)
                (new Interval(70, 80), new ConstantDuration(30)), // 20 (20 = 67%*30) time units gone. Arrival is 90 (70+20)
            ];
        DurationDistributionSelector selector = new(distributions, 1);

        bool canGet = selector.TryGetNextTime(15, _random, out int? arrival, false);
        canGet.Should().BeFalse();
        arrival.Should().BeNull();
    }

    [Fact]
    public void WithoutInitialArrival_And_TwoIntervals()
    {
        List<(Interval, IDurationDistribution)> distributions = [
                (new Interval(10, 20), new ConstantDuration(10)),
                (new Interval(30, 40), new ConstantDuration(10))
            ];
        DurationDistributionSelector selector = new(distributions, 1);

        int numberOfIterations = 1000;
        for (int i = 0; i < numberOfIterations; i++)
        {
            bool canGet = selector.TryGetNextTime(15, _random, out int? arrival, true);
            canGet.Should().BeTrue();
            arrival.Should().NotBeNull();
            arrival.Should().BeInRange(15, 35);
            arrival.Should().NotBeInRange(21, 29);
        }
    }
}
