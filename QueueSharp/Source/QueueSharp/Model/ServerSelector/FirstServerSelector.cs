using QueueSharp.Model.Components;

namespace QueueSharp.Model.ServerSelector;
public class FirstServerSelector : IServerSelector
{
    public bool CanSelectServer(IReadOnlyList<Individual?> servers, out int? selectedServer)
    {
        for (int i = 0; i < servers.Count; i++)
        {
            if (servers[i] is null)
            {
                selectedServer = i;
                return true;
            }
        }
        selectedServer = null;
        return false;
    }
}
