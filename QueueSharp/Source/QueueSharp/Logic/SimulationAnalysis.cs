using QueueSharp.Logic.Helper;
using QueueSharp.Model.Components;
using QueueSharp.Model.Exceptions;
using System.Collections.Frozen;
using System.Text;

namespace QueueSharp.Logic;
public static class SimulationAnalysis
{
    /// <summary>
    /// Calculates a <see cref="SetMetrics"/> summary for the given collection of values,
    /// computing metrics such as mean, variance, min, max, standard deviation, count, and sum.
    /// </summary>
    /// <param name="values">The collection of integer values to compute metrics for.</param>
    /// <returns>A <see cref="SetMetrics"/> instance containing the calculated statistics.</returns>
    public static SetMetrics ComputeSetMetrics(this IEnumerable<int> values)
    {
        double mean = 0;
        double m2 = 0;  // For computing variance
        int? max = null;
        int? min = null;
        int sum = 0;
        int count = 0;

        foreach (int value in values)
        {
            count++;

            // Update min and max
            if (min is null || value < min)
            {
                min = value;
            }
            if (max is null || value > max)
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
            return new SetMetrics(0, 0, 0, 0, 0, 0);
        }

        double variance = m2 / count;

        return new SetMetrics(
            Mean: mean,
            Variance: variance,
            Min: min.GetValueOrDefault(),
            Max: max.GetValueOrDefault(),
            Count: count,
            Sum: sum
        );
    }

    /// <summary>
    /// Calculates a <see cref="SetMetrics"/> summary for the given collection of values,
    /// computing metrics such as mean, variance, min, max, standard deviation, count, and sum.
    /// </summary>
    /// <param name="values">The collection of integer values to compute metrics for.</param>
    /// <returns>A <see cref="SetMetrics"/> instance containing the calculated statistics.</returns>
    public static SetMetrics ComputeSetMetrics(this IEnumerable<double> values)
    {
        double mean = 0;
        double m2 = 0;  // For computing variance
        double? max = null;
        double? min = null;
        double sum = 0;
        int count = 0;

        foreach (double value in values)
        {
            count++;

            // Update min and max
            if (min is null || value < min)
            {
                min = value;
            }
            if (max is null || value > max)
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
            return new SetMetrics(0, 0, 0, 0, 0, 0);
        }

        double variance = m2 / count;

        return new SetMetrics(
            Mean: mean,
            Variance: variance,
            Min: min.GetValueOrDefault(),
            Max: max.GetValueOrDefault(),
            Count: count,
            Sum: sum
        );
    }

    /// <summary>
    /// Merges multiple <see cref="SetMetrics"/> instances into a single summary,
    /// equivalent to calculating metrics over all combined data points in <paramref name="setMetricsSet"/>.
    /// </summary>
    /// <param name="setMetricsSet">The collection of <see cref="SetMetrics"/> instances to merge.</param>
    /// <returns>A new <see cref="SetMetrics"/> with aggregated statistics.</returns>
    public static MetricsAggregation Merge(this IEnumerable<SetMetrics> setMetricsSet)
    {
        return new MetricsAggregation(
            Mean: setMetricsSet.Select(x => x.Mean).ComputeSetMetrics(),
            Variance: setMetricsSet.Select(x => x.Variance).ComputeSetMetrics(),
            Min: setMetricsSet.Select(x => x.Min).ComputeSetMetrics(),
            Max: setMetricsSet.Select(x => x.Max).ComputeSetMetrics(),
            Count: setMetricsSet.Select(x => x.Count).ComputeSetMetrics(),
            Sum: setMetricsSet.Select(x => x.Sum).ComputeSetMetrics());
    }

    public static SimulationReport GetSimulationReport(IEnumerable<NodeVisitRecord> nodeVisitRecords, int? minTime = null, int? maxTime = null)
    {
        FrozenDictionary<string, SimulationNodeReport> simulationReport = nodeVisitRecords
            .Where(x => (!minTime.HasValue || x.ArrivalTime >= minTime) &&
                (!maxTime.HasValue || maxTime >= x.ArrivalTime))
            .GroupBy(x => x.Node.Id)
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
                SetMetrics blockedDurationMetrics = nodeServiceRecords
                    .OfType<NodeServiceRecord>()
                    .Select(x => x.BlockDuration)
                    .ComputeSetMetrics();
                int baulkedAtArrival = nodeServiceRecords
                    .OfType<BaulkingAtArrival>()
                    .Count();
                int baulkedAtService = nodeServiceRecords
                    .OfType<BaulkingAtStartService>()
                    .Count();
                return new SimulationNodeReport(waitingDurationMetrics,
                    serviceDurationMetrics,
                    blockedDurationMetrics,
                    baulkedAtArrival,
                    baulkedAtService);
            });
        return new SimulationReport(simulationReport);
    }

    public static FrozenDictionary<string, SimulationAggregationNodeReport> Merge(IEnumerable<SimulationReport> simulationReports)
    {
        HashSet<string> nodeIds = simulationReports
            .SelectMany(x => x.NodeReportsByNodeId.Keys)
            .Distinct()
            .ToHashSet();
        Dictionary<string, SimulationAggregationNodeReport> mergedNodeReports = [];
        foreach (string nodeId in nodeIds)
        {
            SimulationNodeReport[] simulationNodeReports = simulationReports
                .Select(x => x.NodeReportsByNodeId[nodeId])
                .ToArray();
            SimulationAggregationNodeReport mergedNodeReport = new(WaitingTimeMetrics: simulationNodeReports.Select(x => x.WaitingTimeMetrics).Merge(),
                ServiceDurationMetrics: simulationNodeReports.Select(x => x.ServiceDurationMetrics).Merge(),
                BlockDurationMetrics: simulationNodeReports.Select(x => x.BlockDurationMetrics).Merge(),
                BaulkdedIndividualsAtArrival: simulationNodeReports.Select(x => x.BaulkdedIndividualsAtArrival).ComputeSetMetrics(),
                BaulkdedIndividualsAtServiceStart: simulationNodeReports.Select(x => x.BaulkdedIndividualsAtServiceStart).ComputeSetMetrics());
            mergedNodeReports.Add(nodeId, mergedNodeReport);
        }
        return mergedNodeReports.ToFrozenDictionary();
    }

    internal static string ToCsv(IEnumerable<NodeVisitRecord> nodeVisitRecords)
    {
        StringBuilder stringBuilder = new();
        stringBuilder.AppendLine("Arrival Time\tNode Id\tIndividual It\tCohort Id\tQueue Size at Arrival\tBaulking\tService Start Time\tService End Time\tExit Time\tQueue Size at Exit\tDestination Node Id");
        foreach (NodeVisitRecord nodeVisitRecord in nodeVisitRecords.OrderBy(x => x.ArrivalTime))
        {
            stringBuilder.Append($"{nodeVisitRecord.ArrivalTime}\t{nodeVisitRecord.Node.Id}\t{nodeVisitRecord.Individual.Id}\t{nodeVisitRecord.Individual.Cohort.Id}\t{nodeVisitRecord.QueueSizeAtArrival}");
            switch (nodeVisitRecord)
            {
                case NodeServiceRecord nodeServiceRecord:
                    stringBuilder.Append($"\t\t{nodeServiceRecord.ServiceStartTime}\t{nodeServiceRecord.ServiceEndTime}\t{nodeServiceRecord.ExitTime}\t{nodeServiceRecord.QueueSizeAtExit}\t{nodeServiceRecord.Destination?.Id ?? ""}");
                    break;
                case BaulkingAtArrival:
                    stringBuilder.Append("\tBaulked at Arrival");
                    break;
                case BaulkingAtStartService baulking:
                    stringBuilder.Append($"\tBaulked at Service Start\t{baulking.ServiceStartTime}");
                    break;
                default:
                    throw new NotImplementedEventException(nodeVisitRecord.GetType().Name);
            }
            stringBuilder.Append(Environment.NewLine);
        }
        return stringBuilder.ToString();
    }

    internal static string GetQueueLengthPerNodeOverTimeCsv(IEnumerable<NodeVisitRecord> nodeVisitRecords)
    {
        Dictionary<string, Dictionary<int, int>> queueDeltaPerNodeAndTime = [];
        foreach (NodeVisitRecord nodeVisitRecord in nodeVisitRecords)
        {
            if (!queueDeltaPerNodeAndTime.TryGetValue(nodeVisitRecord.Node.Id, out Dictionary<int, int>? queueDeltaPerTime))
            {
                queueDeltaPerTime = [];
                queueDeltaPerNodeAndTime.Add(nodeVisitRecord.Node.Id, queueDeltaPerTime);
            }

            switch (nodeVisitRecord)
            {
                case NodeServiceRecord nodeServiceRecord:
                    queueDeltaPerTime.Increment(nodeVisitRecord.ArrivalTime);
                    queueDeltaPerTime.Decrement(nodeServiceRecord.ExitTime);

                    // In case of blockage add a 0 delta marker to indicate end service time without exiting
                    _ = queueDeltaPerTime.TryAdd(nodeServiceRecord.ServiceEndTime, 0);
                    break;
                case BaulkingAtArrival:
                    break;
                case BaulkingAtStartService baulking:
                    queueDeltaPerTime.Increment(nodeVisitRecord.ArrivalTime);
                    queueDeltaPerTime.Decrement(baulking.ServiceStartTime);
                    break;
                default:
                    throw new NotImplementedEventException(nodeVisitRecord.GetType().Name);
            }
        }

        StringBuilder stringBuilder = new();
        stringBuilder.AppendLine("Node\tTime\tQueue Length");
        foreach (string nodeId in queueDeltaPerNodeAndTime.Keys)
        {
            int queueLength = 0;
            stringBuilder.AppendLine($"{nodeId}\t0\t{queueLength}");
            foreach ((int time, int delta) in queueDeltaPerNodeAndTime[nodeId].OrderBy(x => x.Key))
            {
                queueLength += delta;
                stringBuilder.AppendLine($"{nodeId}\t{time}\t{queueLength}");
            }
        }

        return stringBuilder.ToString();
    }
}
