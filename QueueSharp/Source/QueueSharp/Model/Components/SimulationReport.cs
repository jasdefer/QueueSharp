using System.Collections.Frozen;

namespace QueueSharp.Model.Components;

public record SetMetrics(
    double Mean,
    double Variance,
    double Min,
    double Max,
    double StandardDeviation,
    int Count,
    int Sum);

public record SimulationReport(FrozenDictionary<Node, SimulationNodeReport> NodeReports)
{
    public int BaulkdedIndividualsAtArrival => NodeReports.Sum(x => x.Value.BaulkdedIndividualsAtArrival);
    public int BaulkdedIndividualsAtServiceStart => NodeReports.Sum(x => x.Value.BaulkdedIndividualsAtServiceStart);
    public int BaulkedIndividuals => BaulkdedIndividualsAtArrival + BaulkdedIndividualsAtServiceStart;
    public int ServiceIndividuals => NodeReports.Sum(x => x.Value.ServedIndividuals);
};

public record SimulationNodeReport(SetMetrics WaitingTimeMetrics,
    SetMetrics ServiceDurationMetrics,
    int BaulkdedIndividualsAtArrival,
    int BaulkdedIndividualsAtServiceStart)
{
    public int ServedIndividuals => ServiceDurationMetrics.Count;
    public int ServiceDuration => ServiceDurationMetrics.Sum;
    public int BaulkedIndividuals => BaulkdedIndividualsAtArrival + BaulkdedIndividualsAtServiceStart;
};
