using System.Diagnostics;

namespace QueueSharp.Model.Components;

[DebuggerDisplay("{Origin.Id} to {Destination.Id}")]
public record Arc(Node Origin, Node Destination);

public record WeightedArc(Node Origin, Node Destination, double Weight = 1) : Arc(Origin, Destination);