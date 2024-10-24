using QueueSharp.Logic;
using QueueSharp.Model.Components;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace QueueSharpUnitTests.Logic;
public class SimulationAnalysisTests
{
    [Fact]
    public void ComputeSetMetricsTest()
    {
        // Arrange: Input collection and expected results
        int[] values = [ 1, 2, 3, 4, 5 ];

        // Expected values for this input
        double expectedMean = 3.0;
        double expectedVariance = 2.0;
        double expectedMin = 1;
        double expectedMax = 5;
        double expectedStandardDeviation = Math.Sqrt(expectedVariance);

        // Act: Call the method under test
        SetMetrics result = SimulationAnalysis.ComputeSetMetrics(values);

        // Assert: Check if result matches the expected values
        result.Mean.Should().BeApproximately(expectedMean, 0.01);
        result.Variance.Should().BeApproximately(expectedVariance, 0.01);
        result.Min.Should().Be(expectedMin);
        result.Max.Should().Be(expectedMax);
        result.StandardDeviation.Should().BeApproximately(expectedStandardDeviation, 0.01);
        result.Count.Should().Be(values.Length);
        result.Sum.Should().Be(15);
    }
}
