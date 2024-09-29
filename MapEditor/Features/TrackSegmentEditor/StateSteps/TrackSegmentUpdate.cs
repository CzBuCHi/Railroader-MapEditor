﻿using MapEditor.Extensions;
using MapEditor.Features.Abstract.StateSteps;
using Track;

namespace MapEditor.Features.TrackSegmentEditor.StateSteps;

public sealed record TrackSegmentUpdate(string Id) : IStateStep
{
    private string?             _A;
    private string?             _B;
    private string?             _GroupId;
    private int?                _Priority;
    private int?                _SpeedLimit;
    private TrackSegment.Style? _Style;
    private TrackClass?         _TrackClass;

    public string?             A          { get; init; }
    public string?             B          { get; init; }
    public string?             GroupId    { get; init; }
    public int?                Priority   { get; init; }
    public int?                SpeedLimit { get; init; }
    public TrackSegment.Style? Style      { get; init; }
    public TrackClass?         TrackClass { get; init; }

    public void Do() {
        if (A == null && B == null && GroupId == null && Priority == null && SpeedLimit == null && Style == null && TrackClass == null) {
            return;
        }

        var segment = Graph.Shared.GetSegment(Id);
        if (segment == null) {
            return;
        }

        if (A != null) {
            _A = segment.a.id;
            segment.a = Graph.Shared.GetNode(A)!;
        }

        if (B != null) {
            _B = segment.b.id;
            segment.b = Graph.Shared.GetNode(B)!;
        }

        if (A != null || B != null) {
            segment.InvalidateCurve();
        }

        if (GroupId != null) {
            _GroupId = segment.groupId;
            segment.groupId = GroupId;
        }

        if (Priority != null) {
            _Priority = segment.priority;
            segment.priority = Priority.Value;
        }

        if (SpeedLimit != null) {
            _SpeedLimit = segment.speedLimit;
            segment.speedLimit = SpeedLimit.Value;
        }

        if (Style != null) {
            _Style = segment.style;
            segment.style = Style.Value;
        }

        if (TrackClass != null) {
            _TrackClass = segment.trackClass;
            segment.trackClass = TrackClass.Value;
        }

        MapEditorPlugin.PatchEditor!.AddOrUpdateSegment(segment);
    }

    public void Undo() {
        if (_A == null && _B == null && _GroupId == null && _Priority == null && _SpeedLimit == null && _Style == null && _TrackClass == null) {
            return;
        }

        var segment = Graph.Shared.GetSegment(Id);
        if (segment == null) {
            return;
        }

        if (_A != null) {
            segment.a = Graph.Shared.GetNode(_A)!;
        }

        if (_B != null) {
            segment.b = Graph.Shared.GetNode(_B)!;
        }

        if (_A != null || _B != null) {
            segment.InvalidateCurve();
        }

        if (_GroupId != null) {
            segment.groupId = _GroupId;
        }

        if (_Priority != null) {
            segment.priority = _Priority.Value;
        }

        if (_SpeedLimit != null) {
            segment.speedLimit = _SpeedLimit.Value;
        }

        if (_Style != null) {
            segment.style = _Style.Value;
        }

        if (_TrackClass != null) {
            segment.trackClass = _TrackClass.Value;
        }

        MapEditorPlugin.PatchEditor!.AddOrUpdateSegment(segment);
    }
}
