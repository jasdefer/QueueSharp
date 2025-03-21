﻿using QueueSharp.Model.Components;
using System.Diagnostics;

namespace QueueSharp.Model.Events;

[DebuggerDisplay("{Timestamp}: Arrival  at {Node.Id} from {Individual.Id}")]
internal record ArrivalEvent(int Timestamp,
    Individual Individual,
    SimulationNode Node) : IEvent;
