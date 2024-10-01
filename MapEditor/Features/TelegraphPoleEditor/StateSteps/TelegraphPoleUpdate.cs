using HarmonyLib;
using MapEditor.Extensions;
using MapEditor.Features.Abstract.StateSteps;
using SimpleGraph.Runtime;
using TelegraphPoles;
using UnityEngine;

namespace MapEditor.Features.TelegraphPoleEditor.StateSteps;

public sealed record TelegraphPoleUpdate(int Id) : IStateStep
{
    private Vector3? _Position;
    private Vector3? _EulerAngles;

    public Vector3? Position    { get; init; }
    public Vector3? EulerAngles { get; init; }

    private Node GetNode() {
        var manager = Object.FindAnyObjectByType<TelegraphPoleManager>();
        var graph   = Traverse.Create(manager!).Field("_graph")!.GetValue<SimpleGraph.Runtime.SimpleGraph>();
        return graph.NodeForId(Id);
    }

    public void Do() {
        if (Position == null && EulerAngles == null) {
            return;
        }

        var node = GetNode();
        if (Position != null) {
            _Position = node.position.Clone();
            node.position = Position.Value.Clone();
        }

        if (EulerAngles != null) {
            _EulerAngles = node.eulerAngles.Clone();
            node.eulerAngles = EulerAngles.Value.Clone();
        }

        MapEditorPlugin.PatchEditor!.AddOrUpdateTelegraphPole(Id, node.position, node.eulerAngles, node.tag);
    }

    public void Undo() {
        if (_Position == null && _EulerAngles == null) {
            return;
        }

        var node = GetNode();
        if (_Position != null) {
            node.position = _Position.Value;
        }

        if (_EulerAngles != null) {
            node.eulerAngles = _EulerAngles.Value;
        }

        MapEditorPlugin.PatchEditor!.RemoveTelegraphPole(Id);
    }

#if DEBUG
    public string DoText {
        get {
            var node = GetNode();
            return "TelegraphPoleUpdate { " +
                   (Position != null ? $"Position = {node.position} -> {Position}, " : "") +
                   (EulerAngles != null ? $"{nameof(EulerAngles)} = " + node.eulerAngles + " -> " + EulerAngles + ", " : "") +
                   " }";
        }
    }

    public string UndoText {
        get {
            var node = GetNode();
            return "TelegraphPoleUpdate { " +
                   (Position != null ? $"Position = {node.position} -> {Position}, " : "") +
                   (EulerAngles != null ? $"EulerAngles = {node.eulerAngles} -> {EulerAngles}, " : "") +
                   " }";
        }
    }
#endif
}
