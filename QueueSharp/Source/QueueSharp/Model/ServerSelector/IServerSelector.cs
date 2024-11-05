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
    /// <param name="node">The node from which to select a server.</param>
    /// <param name="selectedServer">
    /// When this method returns true, contains the index of the selected server if a server is available; 
    /// otherwise, null if no server could be selected.
    /// </param>
    /// <returns>True if a server is successfully selected; otherwise, false.</returns>
    bool CanSelectServer(Node node, out int? selectedServer);
}
