using System;
using System.Collections.Generic;
using System.Diagnostics.Eventing.Reader;
using System.Linq;
using System.Runtime.CompilerServices;
using Helpers;
using MapEditor.Extensions;
using MapEditor.Features.Abstract.StateSteps;
using MapEditor.Features.TrackNodeEditor.StateSteps;
using MapEditor.Features.TrackSegmentEditor;
using MapEditor.Features.TrackSegmentEditor.StateSteps;
using MapEditor.Utility;
using Serilog;
using Track;
using UnityEngine;
using Object = UnityEngine.Object;

namespace MapEditor.Features.TrackNodeEditor;

public static class TrackNodeUtility
{
    public static void Show() {
        CameraSelector.shared.ZoomToPoint(MapEditorPlugin.State.TrackNode!.transform.localPosition);
    }

    public static void Remove() {
        // end track node remove:
        // NODE_A --- NODE
        // result:
        // NODE_A

        // simple track node remove:
        // NODE_A --- NODE --- NODE_B
        // result:
        // NODE_A     NODE_B
        // result (connectSegments):
        // NODE_A --- NODE_B

        // switch track node remove:
        // NODE_A ---\
        //            >- NODE --- NODE_C
        // NODE_B ---/
        // result:
        // NODE_A
        //               NODE_C
        // NODE_B
        // result (connectSegments):
        // NODE_A ---\
        //            >- NODE_C
        // NODE_B ---/
        var connectSegments = Input.GetKey(KeyCode.LeftShift) || Input.GetKey(KeyCode.RightShift);

        var node = MapEditorPlugin.State.TrackNode!;
        Log.Information($"Remove node {node.id}; connectSegments = {connectSegments}");

        TrackSegment? enter = null;
        if (connectSegments) {
            Graph.Shared.DecodeSwitchAt(node, out enter, out _, out _);
        }

        var segments = Graph.Shared.SegmentsConnectedTo(node);
        MapEditorPlugin.UpdateState(state => state with { SelectedAsset = null });

        var actions = segments.Select(trackSegment => new TrackSegmentDestroy(trackSegment.id)).Cast<IStateStep>().ToList();
        actions.Add(new TrackNodeDestroy(node.id));

        if (connectSegments) {
            if (segments.Count == 2) {
                var firstSegment = segments.First();
                actions.Add(new TrackSegmentCreate(IdGenerators.TrackSegments.Next(),
                    new TrackSegmentData(firstSegment) {
                        StartId = firstSegment.GetOtherNode(node)!.id,
                        EndId = segments.Last().GetOtherNode(node)!.id
                    })
                );
            } else if (enter != null) {
                var switchNode = enter.GetOtherNode(node)!;
                var createSegments = segments
                                     .Where(o => o != enter)
                                     .Select(branch => new TrackSegmentCreate(IdGenerators.TrackSegments.Next(),
                                         new TrackSegmentData(enter) {
                                             StartId = switchNode.id,
                                             EndId = branch.GetOtherNode(node)!.id
                                         })
                                     );
                actions.AddRange(createSegments);
            }
        }

        MapEditorStateStepManager.NextStep(new CompoundSteps(actions.ToArray()));
        UnityHelpers.CallOnNextFrame(TrackObjectManager.Instance.Rebuild);
    }

    public static void Add() {
        var withoutSegment = Input.GetKey(KeyCode.LeftShift) || Input.GetKey(KeyCode.RightShift);

        var node = MapEditorPlugin.State.TrackNode!;

        if (!Graph.Shared.NodeIsDeadEnd(node, out var direction)) {
            direction = Vector3.Cross(node.transform.forward, Vector3.up);
        }

        var        nid  = IdGenerators.TrackNodes.Next();
        IStateStep step = new TrackNodeCreate(nid, new TrackNodeData(node.transform.position + direction * 2.5f, node.transform.eulerAngles));

        if (!withoutSegment) {
            var sid           = IdGenerators.TrackSegments.Next();
            var createSegment = new TrackSegmentCreate(sid, new TrackSegmentData(Vector3.zero, Vector3.zero, nid, node.id));
            step = new CompoundSteps(step, createSegment);
        }

        MapEditorStateStepManager.NextStep(step);
        var newNode = Graph.Shared.GetNode(nid)!;
        MapEditorPlugin.UpdateState(state => state with { SelectedAsset = newNode });

        UnityHelpers.CallOnNextFrame(() => TrackObjectManager.Instance.Rebuild());
    }

    public static void Split() {
        // simple track node split:
        // NODE_A --- NODE --- NODE_B
        // result:
        // NODE_A --- NODE
        //            NEW_NODE --- NODE_B

        // switch node split:
        // NODE_A ---\
        //            >- NODE --- NODE_C
        // NODE_B ---/
        // result:
        // NODE_A --- NODE
        // NODE_B --- NEW_NODE_1
        //            NEW_NODE_2 --- NODE_C
        var node = MapEditorPlugin.State.TrackNode!;

        var segments = Graph.Shared.SegmentsConnectedTo(node).ToList();

        List<IStateStep> actions = new();

        if (Graph.Shared.DecodeSwitchAt(node, out _, out var segmentA, out var segmentB)) {
            var left = Vector3.Cross(node.transform.forward, Vector3.up) * 0.75f;

            var a = node.transform.position;
            var b = segmentA!.GetOtherNode(node)!.transform.position;
            var c = segmentB!.GetOtherNode(node)!.transform.position;

            if (Intersect(b, a + left, c, a - left)) {
                left = -left;
            }

            UpdateSegment(segmentA, true, left);
            UpdateSegment(segmentB, true, -left);
        } else {
            UpdateSegment(segments[1]!, false, Vector3.zero);
        }

        MapEditorStateStepManager.NextStep(new CompoundSteps(actions.ToArray()));
        return;

        void UpdateSegment(TrackSegment trackSegment, bool isSwitch, Vector3 offset) {
            var endIsA = trackSegment.EndForNode(node) == TrackSegment.End.A;

            var nid = IdGenerators.TrackNodes.Next();

            var p     = trackSegment.GetParameter(5, node);
            var point = trackSegment.Curve.GetPoint(p).GameToWorld();

            var forward = node.transform.forward * (isSwitch ? 2 : 5);
            if (Vector3.Angle(node.transform.forward, point - node.transform.position) > Math.PI) {
                forward = -forward;
            }

            actions.Add(new TrackNodeCreate(nid, new TrackNodeData(node.transform.position + forward + offset, node.transform.eulerAngles)));

            var a = endIsA ? trackSegment.b.id : nid;
            var b = endIsA ? nid : trackSegment.a.id;

            actions.Add(new TrackSegmentUpdate(trackSegment.id) { A = a, B = b });
        }

        bool Intersect(Vector3 a, Vector3 b, Vector3 c, Vector3 d) {
            var denominator = (b.x - a.x) * (d.z - c.z) - (b.z - a.z) * (d.x - c.x);

            if (Mathf.Abs(denominator) < Mathf.Epsilon) {
                return false;
            }

            var numerator1 = (a.z - c.z) * (d.x - c.x) - (a.x - c.x) * (d.z - c.z);
            var numerator2 = (a.z - c.z) * (b.x - a.x) - (a.x - c.x) * (b.z - a.z);

            var t1 = numerator1 / denominator;
            var t2 = numerator2 / denominator;

            return t1 is >= 0 and <= 1 && t2 is >= 0 and <= 1;
        }
    }

    public static TrackNodeData Destroy(TrackNode trackNode) {
        var trackNodeData = new TrackNodeData(trackNode);
        Object.Destroy(trackNode.gameObject);
        MapEditorPlugin.PatchEditor!.RemoveNode(trackNode.id);
        return trackNodeData;
    }

    public static TrackNode Create(string id, TrackNodeData trackNodeData) {
        var gameObject = new GameObject(id);
        gameObject.SetActive(false);
        gameObject.transform.parent = Graph.Shared.transform;
        var trackNode = gameObject.AddComponent<TrackNode>();

        trackNode.id = id;
        trackNode.transform.position = trackNodeData.Position;
        trackNode.transform.eulerAngles = trackNodeData.EulerAngles;
        gameObject.SetActive(true);

        trackNode.flipSwitchStand = trackNodeData.FlipSwitchStand;
        trackNode.isThrown = trackNodeData.IsThrown;

        Graph.Shared.AddNode(trackNode);
        MapEditorPlugin.PatchEditor!.AddOrUpdateNode(trackNode);
        return trackNode;
    }
}
