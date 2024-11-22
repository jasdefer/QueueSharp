using QueueSharp.Logic.Helper;
using QueueSharp.Model.Components;
using QueueSharp.Model.Exceptions;
using System.Collections.Frozen;
using System.Text;

namespace QueueSharp.Logic;

/// <summary>
/// Analyze the results of one are many simulation runs.
/// </summary>
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

    /// <summary>
    /// Create a <see cref="SimulationReport"/>.
    /// </summary>
    /// <param name="nodeVisitRecords">Represent the simulation result with all events computed during a simulation run.</param>
    /// <param name="minTime">Only visit records with an arrival after this time are considered.</param>
    /// <param name="maxTime">Only visit records with an arrival before this time are considered.</param>
    public static SimulationReport GetSimulationReport(IEnumerable<NodeVisitRecord> nodeVisitRecords, int? minTime = null, int? maxTime = null)
    {
        FrozenDictionary<string, SimulationNodeReport> simulationReport = nodeVisitRecords
            .Where(x => (!minTime.HasValue || x.ArrivalTime >= minTime) &&
                (!maxTime.HasValue || maxTime >= x.ArrivalTime))
            .GroupBy(x => x.NodeId)
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
                    .Where(x => x > 0)
                    .ComputeSetMetrics();
                int rejectedAtArrival = nodeServiceRecords
                    .OfType<RejectionAtArrival>()
                    .Count();
                int rejectedAtService = nodeServiceRecords
                    .OfType<RejectionAtStartService>()
                    .Count();
                return new SimulationNodeReport(waitingDurationMetrics,
                    serviceDurationMetrics,
                    blockedDurationMetrics,
                    rejectedAtArrival,
                    rejectedAtService);
            });
        return new SimulationReport(simulationReport);
    }

    /// <summary>
    /// Merge multiple <see cref="SimulationReport"/> and compute summarizing metrics for each node.
    /// </summary>
    /// <param name="simulationReports"></param>
    /// <returns>Returns a <see cref="SimulationAggregationNodeReport"/> for each node in the simulation. The node id is the key of the dictionary.</returns>
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
                RejectedIndividualsAtArrival: simulationNodeReports.Select(x => x.RejectedIndividualsAtArrival).ComputeSetMetrics(),
                RejectedIndividualsAtServiceStart: simulationNodeReports.Select(x => x.RejectedIndividualsAtServiceStart).ComputeSetMetrics());
            mergedNodeReports.Add(nodeId, mergedNodeReport);
        }
        return mergedNodeReports.ToFrozenDictionary();
    }

    /// <summary>
    /// Get a string returning all node visit records in a csv format with a \t as the separator.
    /// </summary>
    /// <param name="nodeVisitRecords"></param>
    public static string ToCsv(IEnumerable<NodeVisitRecord> nodeVisitRecords)
    {
        StringBuilder stringBuilder = new();
        stringBuilder.AppendLine("Arrival Time\tNode Id\tIndividual It\tCohort Id\tQueue Size at Arrival\tRejection\tService Start Time\tService End Time\tExit Time\tQueue Size at Exit\tDestination Node Id");
        foreach (NodeVisitRecord nodeVisitRecord in nodeVisitRecords.OrderBy(x => x.ArrivalTime))
        {
            stringBuilder.Append($"{nodeVisitRecord.ArrivalTime}\t{nodeVisitRecord.NodeId}\t{nodeVisitRecord.Individual.Id}\t{nodeVisitRecord.Individual.Cohort.Id}\t{nodeVisitRecord.QueueSizeAtArrival}");
            switch (nodeVisitRecord)
            {
                case NodeServiceRecord nodeServiceRecord:
                    stringBuilder.Append($"\t\t{nodeServiceRecord.ServiceStartTime}\t{nodeServiceRecord.ServiceEndTime}\t{nodeServiceRecord.ExitTime}\t{nodeServiceRecord.QueueSizeAtExit}\t{nodeServiceRecord.DestinationNodeId ?? ""}");
                    break;
                case RejectionAtArrival:
                    stringBuilder.Append("\tRejected at Arrival");
                    break;
                case RejectionAtStartService rejection:
                    stringBuilder.Append($"\tRejected at Service Start\t{rejection.ServiceStartTime}");
                    break;
                default:
                    throw new NotImplementedEventException(nodeVisitRecord.GetType().Name);
            }
            stringBuilder.Append(Environment.NewLine);
        }
        return stringBuilder.ToString();
    }

    /// <summary>
    /// Return a csv formatted string using a \t as a separator. Every event changing the queue length of a node is stored in a table.
    /// </summary>
    /// <param name="nodeVisitRecords"></param>
    public static string GetQueueLengthPerNodeOverTimeCsv(IEnumerable<NodeVisitRecord> nodeVisitRecords)
    {
        Dictionary<string, Dictionary<int, int>> queueDeltaPerNodeAndTime = [];
        foreach (NodeVisitRecord nodeVisitRecord in nodeVisitRecords)
        {
            if (!queueDeltaPerNodeAndTime.TryGetValue(nodeVisitRecord.NodeId, out Dictionary<int, int>? queueDeltaPerTime))
            {
                queueDeltaPerTime = [];
                queueDeltaPerNodeAndTime.Add(nodeVisitRecord.NodeId, queueDeltaPerTime);
            }

            switch (nodeVisitRecord)
            {
                case NodeServiceRecord nodeServiceRecord:
                    queueDeltaPerTime.Increment(nodeVisitRecord.ArrivalTime);
                    queueDeltaPerTime.Decrement(nodeServiceRecord.ExitTime);

                    // In case of blockage add a 0 delta marker to indicate end service time without exiting
                    _ = queueDeltaPerTime.TryAdd(nodeServiceRecord.ServiceEndTime, 0);
                    break;
                case RejectionAtArrival:
                    break;
                case RejectionAtStartService rejection:
                    queueDeltaPerTime.Increment(nodeVisitRecord.ArrivalTime);
                    queueDeltaPerTime.Decrement(rejection.ServiceStartTime);
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
