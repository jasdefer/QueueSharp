using QueueSharp.Model.Components;

namespace QueueSharp.Model.ServerSelector;
public interface IServerSelector
{
    bool CanSelectServer(Node node, out int? selectedServer);
}
