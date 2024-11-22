using System.Diagnostics;

namespace QueueSharp.Model.Components;

/// <summary>
/// Represents an arc connecting two nodes.
/// </summary>
/// <param name="Origin"></param>
/// <param name="Destination"></param>
[DebuggerDisplay("{Origin.Id} to {Destination.Id}")]
public record Arc(Node Origin, Node Destination);

/// <summary>
/// Represents an arc connecting two nodes.
/// </summary>
/// <param name="Origin"></param>
/// <param name="Destination"></param>
/// <param name="Weight">A weight used for random selection or other purposes.</param>
public record WeightedArc(Node Origin, Node Destination, double Weight = 1) : Arc(Origin, Destination);