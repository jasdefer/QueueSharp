namespace QueueSharp.Model.Components;
internal class Node
{
    public required string Id { get; init; }
    public int ServerCount { get; private set; }
    public Individual?[] ServingIndividuals { get; private set; } = [];
    public int? QueueCapacity { get; set; }
    public List<Individual> Queue { get; } = [];
    public List<Individual> OverflowQueue { get; } = [];

    public Node(string id, int serverCount)
    {
        Id = id;
        SetServerCount(serverCount);
    }

    public void SetServerCount(int serverCount)
    {
        ServerCount = serverCount;
        ServingIndividuals = new Individual?[serverCount];
    }

    public override int GetHashCode()
    {
        return Id.GetHashCode();
    }

    public bool IsQueueFull => Queue.Count >= QueueCapacity;
    public bool IsQueueEmpty => Queue.Count == 0;
}
