using QueueSharp.Model.Components;

namespace QueueSharp.Model.ServerSelector;
internal interface IServerSelector
{
    bool CanSelectServer(Node node, out int? selectedServer);
}
