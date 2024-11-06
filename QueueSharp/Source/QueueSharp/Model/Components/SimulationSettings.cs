using System.Diagnostics;

namespace QueueSharp.Model.Components;

[DebuggerDisplay("MaxTime {MaxTime}")]
public record SimulationSettings(int? MaxTime);