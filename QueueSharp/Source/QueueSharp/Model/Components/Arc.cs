using System.Diagnostics;

namespace QueueSharp.Model.Components;

[DebuggerDisplay("{Origin.Id} to {Destination.Id}")]
internal record Arc(Node Origin, Node Destination);

internal record WeightedArc(Node Origin, Node Destination, double Weight = 1) : Arc(Origin, Destination);