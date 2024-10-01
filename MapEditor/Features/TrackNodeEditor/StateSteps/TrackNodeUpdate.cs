using MapEditor.Extensions;
using MapEditor.Features.Abstract.StateSteps;
using Track;
using UnityEngine;

namespace MapEditor.Features.TrackNodeEditor.StateSteps;

public sealed record TrackNodeUpdate(string Id) : IStateStep
{
    private Vector3? _LocalPosition;
    private Vector3? _LocalEulerAngles;
    private bool?    _FlipSwitchStand;

    public Vector3? LocalPosition    { get; init; }
    public Vector3? LocalEulerAngles { get; init; }
    public bool?    FlipSwitchStand  { get; init; }

    public void Do() {
        if (LocalPosition == null && LocalEulerAngles == null && FlipSwitchStand == null) {
            return;
        }

        var node = Graph.Shared.GetNode(Id);
        if (node == null) {
            return;
        }

        if (LocalPosition != null) {
            _LocalPosition = node.transform.localPosition.Clone();
            node.transform.localPosition = LocalPosition.Value.Clone();
        }

        if (LocalEulerAngles != null) {
            _LocalEulerAngles = node.transform.localEulerAngles.Clone();
            node.transform.localEulerAngles = LocalEulerAngles.Value.Clone();
        }

        if (FlipSwitchStand != null) {
            _FlipSwitchStand = node.flipSwitchStand;
            node.flipSwitchStand = FlipSwitchStand.Value;
        }

        Graph.Shared.OnNodeDidChange(node);
        MapEditorPlugin.PatchEditor!.AddOrUpdateNode(node);
    }

    public void Undo() {
        if (_LocalPosition == null && _LocalEulerAngles == null && _FlipSwitchStand == null) {
            return;
        }

        var node = Graph.Shared.GetNode(Id);
        if (node == null) {
            return;
        }

        if (_LocalPosition != null) {
            node.transform.localPosition = _LocalPosition.Value;
        }

        if (_LocalEulerAngles != null) {
            node.transform.localEulerAngles = _LocalEulerAngles.Value;
        }

        if (_FlipSwitchStand != null) {
            node.flipSwitchStand = _FlipSwitchStand.Value;
        }

        Graph.Shared.OnNodeDidChange(node);
        MapEditorPlugin.PatchEditor!.AddOrUpdateNode(node);
    }

#if DEBUG
    public string DoText {
        get {
            var node = Graph.Shared.GetNode(Id)!;
            return "TrackNodeUpdate { Id = " + Id + ", " +
                   
                   (LocalPosition != null ? $"LocalPosition = {node.transform.localPosition} -> {LocalPosition}, " : "") +
                   (LocalEulerAngles != null ? $"LocalEulerAngles = {node.transform.eulerAngles} -> {LocalEulerAngles}, " : "") +
                   (FlipSwitchStand != null ? $"FlipSwitchStand = {node.flipSwitchStand} -> {FlipSwitchStand}, " : "") +
                   " }";
        }
    }

    public string UndoText {
        get {
            var node = Graph.Shared.GetNode(Id)!;
            return "TrackNodeUpdate { Id = " + Id + ", " +
                   (_LocalPosition != null ? $"LocalPosition = {node.transform.localPosition} -> {_LocalPosition}, " : "") +
                   (_LocalEulerAngles != null ? $"LocalEulerAngles = {node.transform.eulerAngles} -> {_LocalEulerAngles}, " : "") +
                   (_FlipSwitchStand != null ? $"FlipSwitchStand = {node.flipSwitchStand} -> {_FlipSwitchStand}, " : "") +
                   " }";
        }
    }
#endif
}
