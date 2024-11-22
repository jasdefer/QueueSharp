using System.Diagnostics;

namespace QueueSharp.Model.Components;

/// <summary>
/// Represents an individual moving from node to node in the queueing simulation.
/// </summary>
[DebuggerDisplay("Individual '{Id}' from Cohort '{Cohort.Id}'")]
public class Individual
{
    /// <summary>
    /// The unique id in its <see cref="Cohort"/> of the individual. Individuals from different cohorts can have the same id. The id is only unique within its <see cref="Cohort"/>.
    /// </summary>
    public required int Id { get; init; }
    /// <summary>
    /// The individual has property defined in this <see cref="Cohort"/>.
    /// </summary>
    public required Cohort Cohort { get; set; }
    /// <summary>
    /// The arrival time of the individual at its current node.
    /// </summary>
    internal int? CurrentArrivalTime { get; set; }
}
