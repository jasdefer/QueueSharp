using System.Collections.Frozen;
using System.Diagnostics;

namespace QueueSharp.Model.Components;

/// <summary>
/// Represents a set of metrics describing the distribution of a single set of values.
/// </summary>
/// <param name="Mean">The arithmetic mean of the values in the set.</param>
/// <param name="Variance">The variance of the values in the set, indicating the spread.</param>
/// <param name="Min">The smallest value in the set.</param>
/// <param name="Max">The largest value in the set.</param>
/// <param name="Count">The total number of values in the set.</param>
/// <param name="Sum">The sum of all values in the set.</param>
[DebuggerDisplay("{Mean} Count: {Count} [{Min} {Max}] var: {Variance}")]
public record SetMetrics(
    double Mean,
    double Variance,
    double Min,
    double Max,
    double Count,
    double Sum);

/// <summary>
/// Represents combined statistical metrics calculated from multiple sets of values.
/// </summary>
/// <param name="Mean">Aggregated metrics describing the mean values across sets.</param>
/// <param name="Variance">Aggregated metrics describing the variance values across sets.</param>
/// <param name="Min">Aggregated metrics describing the minimum values across sets.</param>
/// <param name="Max">Aggregated metrics describing the maximum values across sets.</param>
/// <param name="Count">Aggregated metrics describing the count of values across sets.</param>
/// <param name="Sum">Aggregated metrics describing the sum of values across sets.</param>
public record MetricsAggregation(
    SetMetrics Mean,
    SetMetrics Variance,
    SetMetrics Min,
    SetMetrics Max,
    SetMetrics Count,
    SetMetrics Sum);

/// <summary>
/// Represents aggregated metrics and outcomes from a single simulation run.
/// </summary>
/// <param name="NodeReportsByNodeId">Contains detailed reports and metrics for each node in the simulation, identified by node ID.</param>
public record SimulationReport(FrozenDictionary<string, SimulationNodeReport> NodeReportsByNodeId)
{
    /// <summary>
    /// Gets the total number of individuals rejected upon arrival across all nodes.
    /// </summary>
    public int RejectedIndividualsAtArrival => NodeReportsByNodeId.Sum(x => x.Value.RejectedIndividualsAtArrival);

    /// <summary>
    /// Gets the total number of individuals rejected at the start of service across all nodes.
    /// </summary>
    public int RejectedIndividualsAtServiceStart => NodeReportsByNodeId.Sum(x => x.Value.RejectedIndividualsAtServiceStart);

    /// <summary>
    /// Gets the total number of individuals rejected across all nodes.
    /// </summary>
    public int RejectedIndividuals => RejectedIndividualsAtArrival + RejectedIndividualsAtServiceStart;

    /// <summary>
    /// Gets the total number of individuals successfully served across all nodes.
    /// </summary>
    public int ServiceIndividuals => NodeReportsByNodeId.Sum(x => x.Value.ServedIndividuals);
};

/// <summary>
/// Represents detailed metrics for a single simulation node.
/// </summary>
/// <param name="WaitingTimeMetrics">Metrics describing the waiting time for individuals at this node.</param>
/// <param name="ServiceDurationMetrics">Metrics describing the service duration at this node.</param>
/// <param name="BlockDurationMetrics">Metrics describing the duration of blockages at this node.</param>
/// <param name="RejectedIndividualsAtArrival">The number of individuals rejected upon arrival at this node.</param>
/// <param name="RejectedIndividualsAtServiceStart">The number of individuals rejected at the start of service at this node.</param>
public record SimulationNodeReport(
    SetMetrics WaitingTimeMetrics,
    SetMetrics ServiceDurationMetrics,
    SetMetrics BlockDurationMetrics,
    int RejectedIndividualsAtArrival,
    int RejectedIndividualsAtServiceStart)
{
    /// <summary>
    /// Gets the total number of individuals successfully served at this node.
    /// </summary>
    public int ServedIndividuals => (int)ServiceDurationMetrics.Count;

    /// <summary>
    /// Gets the total service duration at this node.
    /// </summary>
    public int ServiceDuration => (int)ServiceDurationMetrics.Sum;

    /// <summary>
    /// Gets the total number of individuals rejected at this node.
    /// </summary>
    public int RejectedIndividuals => RejectedIndividualsAtArrival + RejectedIndividualsAtServiceStart;
};

/// <summary>
/// Represents aggregated metrics across multiple simulation runs for a single node.
/// </summary>
/// <param name="WaitingTimeMetrics">Aggregated metrics describing waiting times across runs for this node.</param>
/// <param name="ServiceDurationMetrics">Aggregated metrics describing service durations across runs for this node.</param>
/// <param name="BlockDurationMetrics">Aggregated metrics describing block durations across runs for this node.</param>
/// <param name="RejectedIndividualsAtArrival">Aggregated metrics for the number of individuals rejected upon arrival across runs for this node.</param>
/// <param name="RejectedIndividualsAtServiceStart">Aggregated metrics for the number of individuals rejected at the start of service across runs for this node.</param>
public record SimulationAggregationNodeReport(
    MetricsAggregation WaitingTimeMetrics,
    MetricsAggregation ServiceDurationMetrics,
    MetricsAggregation BlockDurationMetrics,
    SetMetrics RejectedIndividualsAtArrival,
    SetMetrics RejectedIndividualsAtServiceStart)
{
    /// <summary>
    /// Gets the mean number of individuals successfully served across simulation runs for this node.
    /// </summary>
    public double MeanServedIndividuals => ServiceDurationMetrics.Count.Mean;

    /// <summary>
    /// Gets the mean total service duration across simulation runs for this node.
    /// </summary>
    public double MeanServiceDuration => ServiceDurationMetrics.Sum.Mean;
};
