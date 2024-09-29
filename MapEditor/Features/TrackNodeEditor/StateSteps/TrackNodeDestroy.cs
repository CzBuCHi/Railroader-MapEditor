using MapEditor.Features.Abstract.StateSteps;
using MapEditor.Features.TrackNodeEditor.Visualizer;
using MapEditor.Utility;
using Track;

namespace MapEditor.Features.TrackNodeEditor.StateSteps;

public sealed record TrackNodeDestroy(string Id) : IStateStep
{
    private TrackNodeData? _Data;

    public void Do() {
        var node = Graph.Shared.GetNode(Id);
        if (node == null) {
            return;
        }

        MapEditorPlugin.UpdateState(state => state.TrackNode == node, state => state with { SelectedAsset = null });
        _Data = TrackNodeUtility.Destroy(node);
    }

    public void Undo() {
        if (_Data == null) {
            return;
        }

        var node = TrackNodeUtility.Create(Id, _Data);
        UnityHelpers.CallOnNextFrame(() => TrackNodeVisualizerManager.CreateVisualizer(node));
    }
}
