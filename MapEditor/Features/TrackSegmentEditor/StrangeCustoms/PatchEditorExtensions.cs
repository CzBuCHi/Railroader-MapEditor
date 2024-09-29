using System;
using Newtonsoft.Json.Linq;
using StrangeCustoms.Tracks;
using Track;

namespace MapEditor.Features.TrackSegmentEditor.StrangeCustoms;

public static class PatchEditorExtensions
{
    private static string GetTrestleId(TrackSegment trackSegment) => trackSegment.id + "_Trestle";

    public static void AddOrUpdateAutoTrestle(this PatchEditor patchEditor, TrackSegment trackSegment, Func<AutoTrestleData?, AutoTrestleData> addOrUpdate)
    {
        patchEditor.AddOrUpdateSpliney(GetTrestleId(trackSegment), o =>
        {
            var data = o?.ToObject<AutoTrestleData>();
            data = addOrUpdate(data)!;
            var jData = JObject.FromObject(data);
            jData["handler"] = "StrangeCustoms.AutoTrestleBuilder";
            return jData;
        });
    }

    public static AutoTrestleData? GetAutoTrestle(this PatchEditor patchEditor, TrackSegment trackSegment)
    {
        var splineys = patchEditor.GetSplineys();
        splineys.TryGetValue(GetTrestleId(trackSegment), out var data);
        return data?.ToObject<AutoTrestleData>();
    }

    public static void RemoveAutoTrestle(this PatchEditor patchEditor, TrackSegment trackSegment)
    {
        patchEditor.RemoveSpliney(GetTrestleId(trackSegment));
    }
}
