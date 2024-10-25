using QueueSharp.Logic;
using QueueSharp.Model.Components;

namespace QueueSharpUnitTests.Logic;
public class SimulationAnalysisTests
{
    [Fact]
    public void ComputeSetMetricsIntTest()
    {
        // Arrange: Input collection and expected results
        int[] values = [1, 2, 3, 4, 5];

        // Expected values for this input
        double expectedMean = 3.0;
        double expectedVariance = 2.0;
        double expectedMin = 1;
        double expectedMax = 5;

        // Act: Call the method under test
        SetMetrics result = SimulationAnalysis.ComputeSetMetrics(values);

        // Assert: Check if result matches the expected values
        result.Mean.Should().BeApproximately(expectedMean, 0.01);
        result.Variance.Should().BeApproximately(expectedVariance, 0.01);
        result.Min.Should().Be(expectedMin);
        result.Max.Should().Be(expectedMax);
        result.Count.Should().Be(values.Length);
        result.Sum.Should().Be(15);
    }

    [Fact]
    public void ComputeSetMetricsDoubleTest()
    {
        // Arrange: Input collection and expected results
        double[] values = [1, 2, 3, 4, 5];

        // Expected values for this input
        double expectedMean = 3.0;
        double expectedVariance = 2.0;
        double expectedMin = 1;
        double expectedMax = 5;

        // Act: Call the method under test
        SetMetrics result = SimulationAnalysis.ComputeSetMetrics(values);

        // Assert: Check if result matches the expected values
        result.Mean.Should().BeApproximately(expectedMean, 0.01);
        result.Variance.Should().BeApproximately(expectedVariance, 0.01);
        result.Min.Should().Be(expectedMin);
        result.Max.Should().Be(expectedMax);
        result.Count.Should().Be(values.Length);
        result.Sum.Should().Be(15);
    }

    [Fact]
    public void MergeTest()
    {
        Random random = new(1);
        for (int i = 0; i < 1000; i++)
        {
            int[][] valueSets = new int[random.Next(2, 10)][];
            SetMetrics[] setMetrics = new SetMetrics[valueSets.Length];
            for (int j = 0; j < valueSets.Length; j++)
            {
                valueSets[j] = Enumerable
                    .Range(0, i + 2)
                    .Select(x => random.Next(0, 50))
                    .ToArray();
                setMetrics[j] = valueSets[j].ComputeSetMetrics();
            }
            SetMetrics combinedSetMetrics = valueSets.SelectMany(x => x).ComputeSetMetrics();
            MetricsAggregation metricsAggregation = setMetrics.Merge();
            metricsAggregation.Sum.Sum.Should().Be(combinedSetMetrics.Sum);
            metricsAggregation.Min.Min.Should().Be(combinedSetMetrics.Min);
            metricsAggregation.Max.Max.Should().Be(combinedSetMetrics.Max);
        }
    }
}
