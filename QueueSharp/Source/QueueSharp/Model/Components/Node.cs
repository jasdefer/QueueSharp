namespace QueueSharp.Model.Components;
public record Node(string Id,
    int ServerCount,
    int? QueueCapacity = null);
