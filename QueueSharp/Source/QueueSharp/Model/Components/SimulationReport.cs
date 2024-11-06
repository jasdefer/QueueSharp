using System.Collections.Frozen;
using System.Diagnostics;

namespace QueueSharp.Model.Components;

[DebuggerDisplay("{Mean} Count: {Count} [{Min} {Max}] var: {Variance}")]
public record SetMetrics(
    double Mean,
    double Variance,
    double Min,
    double Max,
    double Count,
    double Sum);

public record MetricsAggregation(
    SetMetrics Mean,
    SetMetrics Variance,
    SetMetrics Min,
    SetMetrics Max,
    SetMetrics Count,
    SetMetrics Sum);

public record SimulationReport(FrozenDictionary<string, SimulationNodeReport> NodeReportsByNodeId)
{
    public int RejectedIndividualsAtArrival => NodeReportsByNodeId.Sum((Func<KeyValuePair<string, SimulationNodeReport>, int>)(x => (int)x.Value.RejectedIndividualsAtArrival));
    public int RejectedIndividualsAtServiceStart => NodeReportsByNodeId.Sum((Func<KeyValuePair<string, SimulationNodeReport>, int>)(x => (int)x.Value.RejectedIndividualsAtServiceStart));
    public int RejectedIndividuals => RejectedIndividualsAtArrival + RejectedIndividualsAtServiceStart;
    public int ServiceIndividuals => NodeReportsByNodeId.Sum(x => x.Value.ServedIndividuals);
};

public record SimulationNodeReport(SetMetrics WaitingTimeMetrics,
    SetMetrics ServiceDurationMetrics,
    SetMetrics BlockDurationMetrics,
    int RejectedIndividualsAtArrival,
    int RejectedIndividualsAtServiceStart)
{
    public int ServedIndividuals => (int)ServiceDurationMetrics.Count;
    public int ServiceDuration => (int)ServiceDurationMetrics.Sum;
    public int RejectedIndividuals => RejectedIndividualsAtArrival + RejectedIndividualsAtServiceStart;
};

public record SimulationAggregationNodeReport(MetricsAggregation WaitingTimeMetrics,
    MetricsAggregation ServiceDurationMetrics,
    MetricsAggregation BlockDurationMetrics,
    SetMetrics RejectedIndividualsAtArrival,
    SetMetrics RejectedIndividualsAtServiceStart)
{
    public double MeanServedIndividuals => ServiceDurationMetrics.Count.Mean;
    public double MeanServiceDuration => ServiceDurationMetrics.Sum.Mean;
};
