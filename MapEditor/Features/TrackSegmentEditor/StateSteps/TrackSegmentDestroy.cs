using MapEditor.Features.Abstract.StateSteps;
using Track;

namespace MapEditor.Features.TrackSegmentEditor.StateSteps;

public sealed record TrackSegmentDestroy(string Id) : IStateStep
{
    private TrackSegmentData? _Data;

    public void Do() {
        var segment = Graph.Shared.GetSegment(Id);
        if (segment == null) {
            return;
        }

        MapEditorPlugin.UpdateState(state => state.TrackSegment == segment, state => state with { SelectedAsset = null });

        _Data = TrackSegmentUtility.Destroy(segment);
    }

    public void Undo() {
        if (_Data == null) {
            return;
        }

        TrackSegmentUtility.Create(Id, _Data);
    }
}
