using QueueSharp.Model.Components;

namespace QueueSharp.Model.ServerSelector;
internal class FirstServerSelector : IServerSelector
{
    public bool CanSelectServer(Node node, out int? selectedServer)
    {
        for (int i = 0; i < node.ServingIndividuals.Length; i++)
        {
            if (node.ServingIndividuals[i] is null)
            {
                selectedServer = i;
                return true;
            }
        }
        selectedServer = null;
        return false;
    }
}
