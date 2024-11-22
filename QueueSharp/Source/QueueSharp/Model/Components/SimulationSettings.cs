using System.Diagnostics;

namespace QueueSharp.Model.Components;

/// <summary>
/// Settings for a simulation run
/// </summary>
/// <param name="MaxTime">The simulation stops after this time.</param>
[DebuggerDisplay("MaxTime {MaxTime}")]
public record SimulationSettings(int? MaxTime);