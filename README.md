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
    D[Process next event]
    Start-->B
    B-->C
    C-- yes -->Stop
    C-- no -->D
    D --> C
```

## Initialize Event List

```mermaid
flowchart TD
```
