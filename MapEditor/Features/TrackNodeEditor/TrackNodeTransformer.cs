using MapEditor.Features.Abstract;
using MapEditor.Features.TrackNodeEditor.StateSteps;
using MapEditor.Utility;
using Track;
using UnityEngine;

namespace MapEditor.Features.TrackNodeEditor;

public sealed class TrackNodeTransformer : IKeyboardTransformer
{
    private const float MoveCoefficient = 0.5f;
    private const float RotateCoefficient = 1f;

    private Vector3? _Initial;

    public void TransformBegin()
    {
        _Initial = MapEditorPlugin.State.TransformMode == TransformMode.Move
            ? MapEditorPlugin.State.TrackNode!.transform.localPosition
            : MapEditorPlugin.State.TrackNode!.transform.localEulerAngles;
    }

    public void TransformComplete()
    {
        var node = MapEditorPlugin.State.TrackNode!;
        TrackNodeUpdate step;

        // reset node position/rotation so MapEditorStateStepManager undo/redo works ...
        var move = MapEditorPlugin.State.TransformMode == TransformMode.Move;
        if (move)
        {
            step = new TrackNodeUpdate(node.id)
            {
                LocalPosition = node.transform.localPosition
            };
            node.transform.localPosition = _Initial!.Value;
        }
        else
        {
            step = new TrackNodeUpdate(node.id)
            {
                LocalEulerAngles = node.transform.localEulerAngles
            };
            node.transform.localEulerAngles = _Initial!.Value;
        }

        MapEditorStateStepManager.NextStep(step);
    }

    public void Transform(float delta, KeyboardTransformDirection direction)
    {
        var node = MapEditorPlugin.State.TrackNode!;
        if (MapEditorPlugin.State.TransformMode == TransformMode.Move)
        {
            KeyboardTransform.Move(node.transform, delta * MoveCoefficient, direction);
        }
        else
        {
            KeyboardTransform.Rotate(node.transform, delta * RotateCoefficient, direction);
        }

        Graph.Shared.OnNodeDidChange(node);
    }
}