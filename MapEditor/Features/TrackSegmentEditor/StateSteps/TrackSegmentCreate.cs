using MapEditor.Features.Abstract.StateSteps;
using Track;

namespace MapEditor.Features.TrackSegmentEditor.StateSteps;

public sealed record TrackSegmentCreate(string Id, TrackSegmentData Data) : IStateStep
{
    public void Do() {
        TrackSegmentUtility.Create(Id, Data);
    }

    public void Undo() {
        var segment = Graph.Shared.GetSegment(Id);
        if (segment == null) {
            return;
        }

        TrackSegmentUtility.Destroy(segment);
    }
}
