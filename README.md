# QueueSharp

A C# library for simulating discrete event systems with a focus on modeling complex queuing systems. It provides tools for constructing, analyzing, and optimizing queues and service nodes, allowing for flexible routing, performance measurement, and system behavior tracking.

## Example network

This network illustrates the flow of attendees entering a large event. There are two entrances: a General Entrance and an Employee Entrance, each leading to different checks like Ticket, Bag, or Batch checks. After completing the necessary checks, attendees can proceed to different areas such as the Main Hall, Stage, or Food Court, depending on their route.

```mermaid
flowchart LR
    A[General Entrance]
    B[Employee Entrance]
    C[Ticket Check]
    D[Bag Check]
    E[Batch Check]
    F[Main Hall Entrance]
    G[Stage Entrance]
    H[Food Court Entrance]
    A-->C
    C-->D
    C-->F
    D-->F
    B-.->E
    E-.->F
    E-.->D
    D-.->F
    F-->G
    F-->H
    F-.->G
    F-.->H
```

# Algorithm

This section describes how the discrete event simulation to handle queuing is implemented.
```mermaid
flowchart TD
    B[Initialize Event List]
    C{{Is Event List Empty?}}
    D[Get next event from List]
    Time[Advance System Clock]
    E[Process event]
    A[Log Processed Event]
    Check{{Max simulation duration reached?}}
    Cancel{{Is cancellation requested?}}
    Start-->B
    B-->C
    C-- no -->D
    D--> Check
    Check -- yes -->Stop
    Check -- no -->Time
    Time-->E
    E-->A
    A-->Cancel
    Cancel -- no -->C
    Cancel -- yes -->Stop
    C-- yes -->Stop
```

- **Initialize Event List**: [Details](#initialize-event-list)
- **Process event**: [Details](#process-event)

## Initialize Event List

```mermaid
flowchart TD
    A[Initialize Empty Event List]
    B[Get the arrival distribution of every node and entity class]
    C[Generate Arrival events for each node and entity class combination]
    D[Add Events to List]
    A-->B
    B-->C
    C-->D 
```

## Process event

Process the event based on it's type.

### Arrival Event

```mermaid
flowchart TD
    Arrive[Individual Arrives at Node]
    CreateNewArrival[Add an arrival event for a new individual to the Event List]
    Arrive --> CreateNewArrival --> Stop
```
- **Individual Arrives at Node**: [Details](#individual-arrives-at-node)
- **Add an arrival event for a new individual to the Event List**: Arrival events are generated whenever an individual arrives at a node. For example, when Individual A arrives at a node, the system determines a random time interval before scheduling the arrival of the next individual (e.g., Individual B) at the same node. This process ensures a continuous flow of arrivals based on random durations.

### Complete Service Event

```mermaid
flowchart TD
    Route[Get routing decision]
    Exit{{Individual exit system?}}
    ExitOrigin[Individual exits origin]
    Stop
    Destination[Individual seeks destination node]
    QueueFull{{Is the queue at the destination node full?}}
    BlockIfFull{{Individual blocks current node}}
    Arrive[Individual Arrives at destination]
    HandleOverflow[Handle Overflow]
    Route-->Exit
    Exit -- yes -->ExitOrigin
    Exit -- no --> Destination
    Destination --> QueueFull
    QueueFull -- yes --> BlockIfFull
    BlockIfFull -- yes --> Stop
    BlockIfFull -- no --> ExitOrigin
    QueueFull -- no --> Arrive
    Arrive --> ExitOrigin
    ExitOrigin --> HandleOverflow
    HandleOverflow --> Stop
```
- **Individual Arrives at destination**: [Details](#individual-arrives-at-node)
- **Individual exits origin**: [Details](#individual-leaves-node)
- **HandleOverflow**: [Details](#handle-overflow-for-node)

## Shared methods

### Individual Arrives at Node

```mermaid
flowchart TD
    A{{Is NodeQueue empty?}}
    C{{Any idle server?}}
    MakeBusy[Mark Server as busy]
    D{{Is queue full?}}
    F[Add new Service Completion Event to the Event List]
    Reject[Reject individual]
    A -- yes --> C
    C -- yes --> MakeBusy
    A -- no --> D
    MakeBusy --> F
    C -- no --> D
    D -- yes --> Reject
    D -- no --> Stop
    F--> Stop
```

### Individual leaves node
```mermaid
flowchart TD
    QueueEmpty{{Is Queue Empty?}}
    MakeServerIdle[Mark the individual serving server idle]
    NewCompletion[Add new Service Completion Event to the Event List]
    CanStart{{Can the next Individual in the queue start the service?}}
    RejectIndividual[Reject Individual]
    QueueEmpty -- yes --> MakeServerIdle
    QueueEmpty -- no --> CanStart
    CanStart -- yes --> NewCompletion
    CanStart -- no --> RejectIndividual
    RejectIndividual --> QueueEmpty
    NewCompletion --> Stop
    MakeServerIdle --> Stop
```

### Handle Overflow for node

```mermaid
flowchart TD
    Initialize[Initialize queue of nodes]
    Add[Add input node to queue]
    IsQueueEmpty{{Is queue empty?}}
    OverflowEmpty{{Is overflow of the node empty?}}
    GetIndividual[Get Individual from overflow]
    GetNext[Get next node from queue]
    IndividualLeavesOrigin[Overflow individual leaves origin]
    IndividualArrivesAtDestination[Overflow individual arrives at destination]
    AddOriginToQueue[Add the origin node to the node queue]
    Initialize --> Add --> IsQueueEmpty
    IsQueueEmpty -- yes --> Stop
    IsQueueEmpty -- no --> GetNext
    GetNext --> OverflowEmpty
    OverflowEmpty -- yes --> Stop
    OverflowEmpty -- no --> GetIndividual
    GetIndividual --> IndividualLeavesOrigin --> IndividualArrivesAtDestination --> AddOriginToQueue 
    AddOriginToQueue --> IsQueueEmpty
```
- **Get Individual from overflow**: If an individual blocks an origin node and wishes to go to a destination node with a full queue, it is stored as an overflow individual at the destination node.
- **Overflow individual leaves origin**: [Details](#individual-leaves-node)
- **Overflow individual arrives at destination**: [Details](#individual-arrives-at-node)