using QueueSharp.Model.Components;

namespace QueueSharp.Model.ServerSelector;

/// <summary>
/// Defines the interface for selecting a server within a specified node.
/// Implementations determine if a suitable server can be selected based on node conditions or availability.
/// </summary>
public interface IServerSelector
{
    /// <summary>
    /// Attempts to select an available server within the specified node.
    /// </summary>
    /// <param name="servers">An array of the servers at the node with the currently served individual. The element of the array is null, if no individual is currently being served at that server.</param>
    /// <param name="selectedServer">
    /// When this method returns true, contains the index of the selected server if a server is available; 
    /// otherwise, null if no server could be selected.
    /// </param>
    /// <returns>True if a server is successfully selected; otherwise, false.</returns>
    bool CanSelectServer(IReadOnlyList<Individual?> servers, out int? selectedServer);
}
