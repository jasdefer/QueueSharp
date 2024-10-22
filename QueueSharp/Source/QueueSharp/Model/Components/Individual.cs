using System.Diagnostics;

namespace QueueSharp.Model.Components;

[DebuggerDisplay("Individual '{Id}' from Cohort '{Cohort.Id}'")]
public class Individual
{
    public required int Id { get; init; }
    public required Cohort Cohort { get; set; }
    public int? CurrentArrivalTime { get; set; }
}
