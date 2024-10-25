using System.Collections.Frozen;

namespace QueueSharp.Model.Components;

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
    public int BaulkdedIndividualsAtArrival => NodeReportsByNodeId.Sum(x => x.Value.BaulkdedIndividualsAtArrival);
    public int BaulkdedIndividualsAtServiceStart => NodeReportsByNodeId.Sum(x => x.Value.BaulkdedIndividualsAtServiceStart);
    public int BaulkedIndividuals => BaulkdedIndividualsAtArrival + BaulkdedIndividualsAtServiceStart;
    public int ServiceIndividuals => NodeReportsByNodeId.Sum(x => x.Value.ServedIndividuals);
};

public record SimulationNodeReport(SetMetrics WaitingTimeMetrics,
    SetMetrics ServiceDurationMetrics,
    int BaulkdedIndividualsAtArrival,
    int BaulkdedIndividualsAtServiceStart)
{
    public int ServedIndividuals => (int)ServiceDurationMetrics.Count;
    public int ServiceDuration => (int)ServiceDurationMetrics.Sum;
    public int BaulkedIndividuals => BaulkdedIndividualsAtArrival + BaulkdedIndividualsAtServiceStart;
};

public record SimulationAggregationNodeReport(MetricsAggregation WaitingTimeMetrics,
    MetricsAggregation ServiceDurationMetrics,
    SetMetrics BaulkdedIndividualsAtArrival,
    SetMetrics BaulkdedIndividualsAtServiceStart)
{
    public double MeanServedIndividuals => ServiceDurationMetrics.Count.Mean;
    public double MeanServiceDuration => ServiceDurationMetrics.Sum.Mean;
};
