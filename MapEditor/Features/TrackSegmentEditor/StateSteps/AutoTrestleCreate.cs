using MapEditor.Features.Abstract.StateSteps;
using MapEditor.Features.TrackSegmentEditor.StrangeCustoms;
using Track;

namespace MapEditor.Features.TrackSegmentEditor.StateSteps;

public sealed record AutoTrestleCreate(string Id, AutoTrestleData Data) : IStateStep
{
    public void Do()
    {
        var segment = Graph.Shared.GetSegment(Id);
        if (segment == null)
        {
            return;
        }

        MapEditorPlugin.PatchEditor!.AddOrUpdateAutoTrestle(segment, _ => Data);
        AutoTrestleUtility.CreateTrestle(segment, Data);
    }

    public void Undo()
    {
        var segment = Graph.Shared.GetSegment(Id);
        if (segment == null)
        {
            return;
        }

        MapEditorPlugin.PatchEditor!.RemoveAutoTrestle(segment);
    }
}
