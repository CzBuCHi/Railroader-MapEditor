using MapEditor.Features.Abstract.StateSteps;
using MapEditor.Features.TrackSegmentEditor.StrangeCustoms;
using Track;

namespace MapEditor.Features.TrackSegmentEditor.StateSteps;

public sealed record AutoTrestleUpdate(string Id) : IStateStep
{
    private AutoTrestle.AutoTrestle.EndStyle? _HeadStyle;
    private AutoTrestle.AutoTrestle.EndStyle? _TailStyle;

    public AutoTrestle.AutoTrestle.EndStyle? HeadStyle { get; init; }
    public AutoTrestle.AutoTrestle.EndStyle? TailStyle { get; init; }

    public void Do() {
        var segment = Graph.Shared.GetSegment(Id);
        if (segment == null) {
            return;
        }

        var data = MapEditorPlugin.PatchEditor!.GetAutoTrestle(segment)!;
        _HeadStyle = data.HeadStyle;
        _TailStyle = data.TailStyle;

        MapEditorPlugin.PatchEditor!.AddOrUpdateAutoTrestle(segment, AutoTrestleUtility.CreateOrUpdate(segment, HeadStyle, TailStyle));
        AutoTrestleUtility.UpdateTrestle(segment);
    }

    public void Undo() {
        var segment = Graph.Shared.GetSegment(Id);
        if (segment == null) {
            return;
        }

        MapEditorPlugin.PatchEditor!.AddOrUpdateAutoTrestle(segment, AutoTrestleUtility.CreateOrUpdate(segment, _HeadStyle, _TailStyle));
        AutoTrestleUtility.UpdateTrestle(segment);
    }
}
