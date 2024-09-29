using System;
using System.Collections.Generic;
using Helpers;
using MapEditor.Extensions;
using MapEditor.Features.Abstract.StateSteps;
using MapEditor.Features.TrackNodeEditor;
using MapEditor.Features.TrackNodeEditor.StateSteps;
using MapEditor.Features.TrackSegmentEditor.StateSteps;
using MapEditor.Utility;
using Track;
using UnityEngine;
using Object = UnityEngine.Object;

namespace MapEditor.Features.TrackSegmentEditor;

internal static class TrackSegmentUtility
{
    public static void InjectNode() {
        // inject node in center of segment:
        // NODE_A  --- NODE_B
        // result:
        // NODE_A  --- NEW_NODE --- NODE_B
        var trackSegment = MapEditorPlugin.State.TrackSegment!;

        var nodeA = trackSegment.a.id;
        var nodeB = trackSegment.b.id;

        var position    = trackSegment.Curve.GetPoint(0.5f).GameToWorld();
        var eulerAngles = trackSegment.Curve.GetRotation(0.5f).eulerAngles;

        MapEditorPlugin.UpdateState(state => state with { SelectedAsset = null });

        var nid  = IdGenerators.TrackNodes.Next();
        var sid = IdGenerators.TrackSegments.Next();

        var actions = new List<IStateStep> {
            new TrackNodeCreate(nid, new TrackNodeData(position, eulerAngles)),
            new TrackSegmentUpdate(trackSegment.id) { A = nodeA, B = nid },
            new TrackSegmentCreate(sid, new TrackSegmentData(trackSegment) { StartId = nid, EndId = nodeB })
        };

        MapEditorStateStepManager.NextStep(new CompoundSteps(actions.ToArray()));
        UnityHelpers.CallOnNextFrame(() => {
            MapEditorPlugin.UpdateState(state => state with {
                SelectedAsset = Graph.Shared.GetNode(nid)
            });
        });
        
    }

    public static TrackSegmentData Destroy(TrackSegment trackSegment) {
        var trackSegmentData = new TrackSegmentData(trackSegment);
        Object.Destroy(trackSegment.gameObject);
        MapEditorPlugin.PatchEditor!.RemoveSegment(trackSegment.id);
        return trackSegmentData;
    }

    public static void Create(string id, TrackSegmentData trackSegmentData) {
        var a = Graph.Shared.GetNode(trackSegmentData.StartId);
        var b = Graph.Shared.GetNode(trackSegmentData.EndId);
        if (a == null || b == null) {
            return;
        }

        var gameObject = new GameObject(id);
        gameObject.SetActive(false);
        gameObject.transform.parent = Graph.Shared.transform;
        var trackSegment = gameObject.AddComponent<TrackSegment>();
        trackSegment.id = id;
        trackSegment.transform.position = trackSegmentData.Position;
        trackSegment.transform.eulerAngles = trackSegmentData.EulerAngles;
        gameObject.SetActive(true);

        trackSegment.a = a;
        trackSegment.b = b;
        trackSegment.style = trackSegmentData.Style;
        trackSegment.trackClass = trackSegmentData.TrackClass;
        trackSegment.priority = trackSegmentData.Priority;
        trackSegment.speedLimit = trackSegmentData.SpeedLimit;
        trackSegment.groupId = trackSegmentData.GroupId!;
        trackSegment.InvalidateCurve();

        Graph.Shared.AddSegment(trackSegment);
        MapEditorPlugin.PatchEditor!.AddOrUpdateSegment(trackSegment);
    }

    public static Action UpdateSegment(string? groupId, int? priority, int? speedLimit, TrackClass? trackClass, TrackSegment.Style? style, AutoTrestle.AutoTrestle.EndStyle trestleHead, AutoTrestle.AutoTrestle.EndStyle trestleTail) {
        return () => {
            var segment = MapEditorPlugin.State.TrackSegment;
            IStateStep step = new TrackSegmentUpdate(segment!.id) {
                GroupId = groupId,
                Priority = priority,
                SpeedLimit = speedLimit,
                TrackClass = trackClass,
                Style = style
            };

            var oldStyle = segment.style;

            if (style != oldStyle) {
                if (style == TrackSegment.Style.Bridge) {
                    var autoTrestleCreate = new AutoTrestleCreate(segment.id, AutoTrestleUtility.CreateAutoTrestleData(segment, trestleHead, trestleTail));
                    step = new CompoundSteps(step, autoTrestleCreate);
                } else {
                    var autoTrestleDestroy = new AutoTrestleDestroy(segment.id);
                    step = new CompoundSteps(step, autoTrestleDestroy);
                }
            } else {
                var autoTrestleUpdate = new AutoTrestleUpdate(segment.id) {
                    HeadStyle = trestleHead,
                    TailStyle = trestleTail
                };
                step = new CompoundSteps(step, autoTrestleUpdate);
            }

            MapEditorStateStepManager.NextStep(step);
            UnityHelpers.CallOnNextFrame(TrackObjectManager.Instance.Rebuild);
        };
    }

    public static void Remove() {
        var trackSegment = MapEditorPlugin.State.TrackSegment!;
        MapEditorPlugin.UpdateState(state => state with { SelectedAsset = null });
        MapEditorStateStepManager.NextStep(new TrackSegmentDestroy(trackSegment.id));
    }
}
