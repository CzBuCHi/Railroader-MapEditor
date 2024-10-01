using Helpers;
using MapEditor.Features.Abstract.StateSteps;
using UnityEngine;

namespace MapEditor.Features.SceneryAssetEditor.StateSteps;

public class SceneryAssetUpdate(SceneryAssetInstance sceneryAssetInstance) : IStateStep
{
    private Vector3? _LocalPosition;
    private Vector3? _LocalEulerAngles;

    public Vector3? LocalPosition    { get; init; }
    public Vector3? LocalEulerAngles { get; init; }

    public void Do() {
        if (LocalPosition == null && LocalEulerAngles == null) {
            return;
        }

        if (LocalPosition != null) {
            _LocalPosition = sceneryAssetInstance.transform.localPosition;
            sceneryAssetInstance.transform.localPosition = LocalPosition.Value;
        }

        if (LocalEulerAngles != null) {
            _LocalEulerAngles = sceneryAssetInstance.transform.localEulerAngles;
            sceneryAssetInstance.transform.localEulerAngles = LocalEulerAngles.Value;
        }

        sceneryAssetInstance.ReloadComponents();
    }

    public void Undo() {
        if (_LocalPosition == null && _LocalEulerAngles == null) {
            return;
        }

        if (_LocalPosition != null) {
            sceneryAssetInstance.transform.localPosition = _LocalPosition.Value;
        }

        if (_LocalEulerAngles != null) {
            sceneryAssetInstance.transform.localEulerAngles = _LocalEulerAngles.Value;
        }

        sceneryAssetInstance.ReloadComponents();
    }

#if DEBUG
    public string DoText =>
        "SceneryAssetUpdate { " +
        (LocalPosition != null ? $"LocalPosition = {sceneryAssetInstance.transform.localPosition} -> {LocalPosition}, " : "") +
        (LocalEulerAngles != null ? $"LocalEulerAngles = {sceneryAssetInstance.transform.localEulerAngles} -> {LocalEulerAngles}, " : "") +
        " }";

    public string UndoText =>
        "SceneryAssetUpdate { " +
        (_LocalPosition != null ? $"LocalPosition = {sceneryAssetInstance.transform.localPosition} -> {_LocalPosition}, " : "") +
        (_LocalEulerAngles != null ? $"LocalEulerAngles = {sceneryAssetInstance.transform.localEulerAngles} -> {_LocalEulerAngles}, " : "") +
        " }";
#endif
}
