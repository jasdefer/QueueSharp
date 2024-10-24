using QueueSharp.Model.Components;
using System.Collections.Frozen;

namespace QueueSharp.Logic;
public static class SimulationAnalysis
{
    public static SetMetrics ComputeSetMetrics(this IEnumerable<int> values)
    {
        double mean = 0;
        double m2 = 0;  // For computing variance
        int max = int.MinValue;
        int min = int.MaxValue;
        int sum = 0;
        int count = 0;

        foreach (int value in values)
        {
            count++;

            // Update min and max
            if (value < min)
            {
                min = value;
            }
            if (value > max)
            {
                max = value;
            }

            // Welford's method to calculate mean and variance in one pass
            double delta = value - mean;
            mean += delta / count;
            m2 += delta * (value - mean);

            sum += value;
        }

        if (count == 0)
        {
            return new SetMetrics(0, 0, 0, 0, 0, 0, 0);
        }

        double variance = m2 / count;
        double standardDeviation = Math.Sqrt(variance);

        return new SetMetrics(
            Mean: mean,
            Variance: variance,
            Min: min,
            Max: max,
            StandardDeviation: standardDeviation,
            Count: count,
            Sum: sum);
    }

    public static SimulationReport GetSimulationReport(IEnumerable<NodeVisitRecord> nodeVisitRecords, int? minTime = null, int? maxTime = null)
    {
        FrozenDictionary<Node, SimulationNodeReport> simulationReport = nodeVisitRecords
            .Where(x => (!minTime.HasValue || x.ArrivalTime >= minTime) &&
                (!maxTime.HasValue || maxTime >= x.ArrivalTime))
            .GroupBy(x => x.Node)
            .ToFrozenDictionary(x => x.Key, nodeServiceRecords => 
            {
                SetMetrics waitingDurationMetrics = nodeServiceRecords
                    .OfType<NodeServiceRecord>() // ToDo: Performance improvements by reducing the double OfType call
                    .Select(y => y.WaitingDuration)
                    .ComputeSetMetrics();
                SetMetrics serviceDurationMetrics = nodeServiceRecords
                    .OfType<NodeServiceRecord>()
                    .Select(x => x.ServiceDuration)
                    .ComputeSetMetrics();
                int baulkedAtArrival = nodeServiceRecords
                    .OfType<BaulkingAtArrival>()
                    .Count();
                int baulkedAtService = nodeServiceRecords
                    .OfType<BaulkingAtStartService>()
                    .Count();
                return new SimulationNodeReport(waitingDurationMetrics,
                    serviceDurationMetrics,
                    baulkedAtArrival,
                    baulkedAtService);
            });
        return new SimulationReport(simulationReport);
    }
}
