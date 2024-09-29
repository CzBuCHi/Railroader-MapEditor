using MapEditor.Features.Abstract;
using MapEditor.Features.SceneryAssetEditor.StateSteps;
using MapEditor.Features.TrackNodeEditor.StateSteps;
using MapEditor.Utility;
using UnityEngine;

namespace MapEditor.Features.SceneryAssetEditor;

public sealed class SceneryAssetTransformer : IKeyboardTransformer
{
    private const float MoveCoefficient   = 0.5f;
    private const float RotateCoefficient = 1f;

    private Vector3? _Initial;

    public void TransformBegin() {
        _Initial = MapEditorPlugin.State.TransformMode == TransformMode.Move
            ? MapEditorPlugin.State.SceneryAssetInstance!.transform.localPosition
            : MapEditorPlugin.State.SceneryAssetInstance!.transform.localEulerAngles;
    }

    public void TransformComplete() {
        var asset = MapEditorPlugin.State.SceneryAssetInstance!;

        SceneryAssetUpdate step;

        // reset node position/rotation so MapEditorStateStepManager undo/redo works ...
        var move = MapEditorPlugin.State.TransformMode == TransformMode.Move;
        if (move) {
            step = new SceneryAssetUpdate(asset) {
                LocalPosition = asset.transform.localPosition
            };
            asset.transform.localPosition = _Initial!.Value;
        } else {
            step = new SceneryAssetUpdate(asset) {
                LocalEulerAngles = asset.transform.localEulerAngles
            };
            asset.transform.localEulerAngles = _Initial!.Value;
        }

        MapEditorStateStepManager.NextStep(step);
    }

    public void Transform(float delta, KeyboardTransformDirection direction) {
        var node = MapEditorPlugin.State.SceneryAssetInstance!;
        if (MapEditorPlugin.State.TransformMode == TransformMode.Move) {
            KeyboardTransform.Move(node.transform, delta * MoveCoefficient, direction);
        } else {
            // game do not like rotation on X or Z axis
            KeyboardTransform.Rotate(node.transform, Vector3.up * delta * RotateCoefficient, direction);
        }
    }
}
