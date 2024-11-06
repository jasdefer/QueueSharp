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

```mermaid
flowchart TD
    B[Initialize Event List]
    C{{Is Event List Empty?}}
    D[Get next event from List]
    Time[Advance System Clock]
    E[Process event]
    A[Log Processed Event]
    Start-->B
    B-->C
    C-- no -->D
    D-->Time
    Time-->E
    E-->A
    A-->C
    C-- yes -->Stop
```

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

Input:

- Entity Class
- Number of arriving entities
- Node

```mermaid
flowchart TD
    A{{Is NodeQueue empty?}}
    C{{Any idle server?}}
    MakeBusy[Mark Server as busy]
    D[Add entities to queue]
    E[Create Service Completion Event]
    F[Add created event to the Event List]
    Stop
    A -- yes --> C
    C -- yes --> MakeBusy
    A -- no --> D
    MakeBusy --> E
    C -- no --> D
    E-->F
    F-->Stop
    D --> Stop
```

### Complete Service Event

```mermaid
flowchart TD
    IsSink{{Is current Node a Sink?}}
    SelectDestination[Select destination Node]
    IsQueueFull{{Is the target node queue full?}}
    AddToOverflowQueue[Add Entity to Overflow Queue of Destination Node]
    CreateArrivalEvent[Create Arrival Event]
    MarkServerIdle[Mark Server Idle]
    IsQueueEmpty{{Is Queue from leaving Node empty?}}
    DequeueEntity[Dequeue entity]
    CreateCompleteServiceEvent[Create Complete Service Event]
    CreateOverflowArrivalEvent[Create Arrival Event for overflow entity]
    IsSink -- yes --> IsQueueEmpty
    IsSink -- no --> SelectDestination
    SelectDestination --> IsQueueFull
    IsQueueFull -- no --> CreateArrivalEvent
    CreateArrivalEvent --> IsQueueEmpty
    IsQueueFull -- yes --> AddToOverflowQueue
    AddToOverflowQueue --> Stop
    IsQueueEmpty -- yes --> MarkServerIdle
    MarkServerIdle --> IsOverflowQueueEmpty
    IsQueueEmpty -- no --> DequeueEntity
    DequeueEntity --> CreateCompleteServiceEvent
    CreateCompleteServiceEvent --> IsOverflowQueueEmpty
    IsOverflowQueueEmpty -- yes --> Stop
    IsOverflowQueueEmpty -- no --> CreateOverflowArrivalEvent
    CreateOverflowArrivalEvent --> Stop
```
