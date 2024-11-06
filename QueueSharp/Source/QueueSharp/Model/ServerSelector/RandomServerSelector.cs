using QueueSharp.Model.Components;

namespace QueueSharp.Model.ServerSelector;
internal class RandomServerSelector : IServerSelector
{
    private readonly Random _random;

    public RandomServerSelector(int? randomSeed = null)
    {
        _random = randomSeed is not null
            ? new Random(randomSeed.Value)
            : new Random();
    }
    public bool CanSelectServer(IReadOnlyList<Individual?> servers, out int? selectedServer)
    {
        IEnumerable<int> indices = Enumerable
            .Range(0, servers.Count)
            .OrderBy(x => _random.NextDouble());
        foreach (int i in indices)
        {
            if (servers is null)
            {
                selectedServer = i;
                return true;
            }
        }
        selectedServer = null;
        return false;
    }
}
