using MapEditor.Features.Abstract;
using MapEditor.Features.TelegraphPoleEditor.Harmony;
using MapEditor.Features.TelegraphPoleEditor.StateSteps;
using MapEditor.Utility;
using UnityEngine;

namespace MapEditor.Features.TelegraphPoleEditor;

public sealed class TelegraphPoleTransformer : IKeyboardTransformer
{
    private const float MoveCoefficient = 0.5f;
    private const float RotateCoefficient = 3f;

    private bool IsMoveTransform => MapEditorPlugin.State.TransformMode == TransformMode.Move;
    private Vector3 _Initial;

    private int NodeId => MapEditorPlugin.State.TelegraphPole!.Id;

    public void TransformBegin()
    {
        var telegraphPole = TelegraphPoleUtility.GetTelegraphPole(NodeId);

        if (IsMoveTransform)
        {
            _Initial = telegraphPole.transform.localPosition;
        }
        else
        {
            _Initial = telegraphPole.transform.localEulerAngles;
        }
    }

    public void TransformComplete()
    {
        var telegraphPole = TelegraphPoleUtility.GetTelegraphPole(NodeId);
        var node = TelegraphPoleUtility.Graph.NodeForId(NodeId)!;

        TelegraphPoleUpdate step;
        // reset pole position/rotation so MapEditorStateStepManager undo/redo works ...
        if (IsMoveTransform)
        {
            var delta = telegraphPole.transform.localPosition - _Initial;
            step = new TelegraphPoleUpdate(NodeId) { Position = node.position + delta };
            telegraphPole.transform.localPosition = _Initial;
        }
        else
        {
            var delta = telegraphPole.transform.localEulerAngles - _Initial;
            step = new TelegraphPoleUpdate(NodeId) { EulerAngles = node.eulerAngles + delta };
            telegraphPole.transform.localEulerAngles = _Initial;
        }

        MapEditorStateStepManager.NextStep(step);
        TelegraphPoleUtility.Manager.Rebuild();
    }

    public void Transform(float delta, KeyboardTransformDirection direction)
    {
        var telegraphPole = TelegraphPoleUtility.GetTelegraphPole(NodeId);
        if (IsMoveTransform)
        {
            KeyboardTransform.Move(telegraphPole.transform, delta * MoveCoefficient, direction);
        }
        else
        {
            KeyboardTransform.Rotate(telegraphPole.transform, delta * RotateCoefficient, direction);
        }
    }
}
