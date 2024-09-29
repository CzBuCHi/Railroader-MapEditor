using MapEditor.Features.Abstract.StateSteps;
using MapEditor.Features.TrackNodeEditor.Visualizer;
using MapEditor.Utility;
using Track;

namespace MapEditor.Features.TrackNodeEditor.StateSteps;

public sealed record TrackNodeCreate(string Id, TrackNodeData Data) : IStateStep
{
    public void Do() {
        var node = TrackNodeUtility.Create(Id, Data);
        MapEditorPlugin.UpdateState(state => state with { SelectedAsset = node });
        UnityHelpers.CallOnNextFrame(() => TrackNodeVisualizerManager.CreateVisualizer(node));
    }

    public void Undo() {
        var trackNode = Graph.Shared.GetNode(Id);
        if (trackNode == null) {
            return;
        }

        MapEditorPlugin.UpdateState(state => state.TrackNode == trackNode, state => state with { SelectedAsset = null });
        TrackNodeUtility.Destroy(trackNode);
    }
}
