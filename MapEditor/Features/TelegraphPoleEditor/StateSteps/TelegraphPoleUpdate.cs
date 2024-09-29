using HarmonyLib;
using MapEditor.Extensions;
using MapEditor.Features.Abstract.StateSteps;
using SimpleGraph.Runtime;
using TelegraphPoles;
using UnityEngine;

namespace MapEditor.Features.TelegraphPoleEditor.StateSteps;

public sealed record TelegraphPoleUpdate(int Id) : IStateStep
{
    private Vector3? _LocalPosition;
    private Vector3? _LocalEulerAngles;

    public Vector3? LocalPosition { get; init; }
    public Vector3? LocalEulerAngles { get; init; }

    private Node GetNode()
    {
        var manager = Object.FindAnyObjectByType<TelegraphPoleManager>();
        var graph = Traverse.Create(manager!).Field("_graph")!.GetValue<SimpleGraph.Runtime.SimpleGraph>();
        return graph.NodeForId(Id);
    }

    public void Do()
    {
        if (LocalPosition == null && LocalEulerAngles == null)
        {
            return;
        }

        var node = GetNode();
        if (LocalPosition != null)
        {
            global::UI.Console.Console.shared.AddLine($"Do LocalPosition: {LocalPosition}");
            _LocalPosition = node.position.Clone();
            node.position = LocalPosition.Value.Clone();
        }

        if (LocalEulerAngles != null)
        {
            global::UI.Console.Console.shared.AddLine($"Do LocalEulerAngles: {LocalEulerAngles}");
            _LocalEulerAngles = node.eulerAngles.Clone();
            node.eulerAngles = LocalEulerAngles.Value.Clone();
        }

        MapEditorPlugin.PatchEditor!.AddOrUpdateTelegraphPole(Id, node.position, node.eulerAngles, node.tag);
    }

    public void Undo()
    {
        if (_LocalPosition == null && _LocalEulerAngles == null)
        {
            return;
        }

        var node = GetNode();
        if (_LocalPosition != null)
        {
            global::UI.Console.Console.shared.AddLine($"Undo LocalPosition: {_LocalPosition}");
            node.position = _LocalPosition.Value;
        }

        if (_LocalEulerAngles != null)
        {
            global::UI.Console.Console.shared.AddLine($"Undo LocalEulerAngles: {_LocalEulerAngles}");
            node.eulerAngles = _LocalEulerAngles.Value;
        }

        MapEditorPlugin.PatchEditor!.RemoveTelegraphPole(Id);
    }
}
