using QueueSharp.Model.Components;

namespace QueueSharp.Model.ServerSelector;

/// <summary>
/// Selects the first available server.
/// </summary>
public class FirstServerSelector : IServerSelector
{
    /// <summary>
    /// Selects the first available server.
    /// </summary>
    /// <param name="servers">The collection of servers and the individual they are currently serving. An element is null, when the server is available.</param>
    /// <param name="selectedServer">The resulting index of the selected server.</param>
    /// <returns>True, if any server can be selected, false if all servers are busy.</returns>
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
