using System;
using System.Collections.Generic;
using System.Linq;
using GalaSoft.MvvmLight.Messaging;
using Helpers;
using MapEditor.Features.Abstract;
using Railloader;
using Track;
using UI.Builder;
using UI.Common;
using UnityEngine;

namespace MapEditor.Features.SceneView;

public sealed class SceneViewDialog(IUIHelper uiHelper) : DialogBase(uiHelper)
{
    #region Manage

    private static SceneViewDialog? _Instance;

    public static void Show(IUIHelper uiHelper) {
        Show(ref _Instance, () => new SceneViewDialog(uiHelper));
    }

    public static void Close() {
        Close(ref _Instance);
    }

    #endregion

    protected override int             WindowWidth    => 800;
    protected override int             WindowHeight   => 600;
    protected override Window.Position WindowPosition => Window.Position.Center;
    protected override string          WindowTitle    => "Map Editor | Scene Viewer";

    protected override void BuildWindow(UIPanelBuilder builder) {
        if (!WorldTransformer.TryGetShared(out var worldTransformer) || worldTransformer == null) {
            builder.AddLabel("Cannot find World");
            return;
        }

        builder.RebuildOnEvent<RebuildSceneViewDialog>();
        builder.VScrollView(scroll => BuildSectionForGameObject(scroll, worldTransformer.gameObject, 0));
    }

    // level => 1-based index (0 is for 'Select ...')
    private readonly Dictionary<int, int> _SelectedComponent = new() { { 0, 0 } };
    private readonly Dictionary<int, int> _SelectedChild     = new() { { 0, 0 } };

    private void BuildSectionForGameObject(UIPanelBuilder builder, GameObject gameObject, int level) {
        var components = gameObject.GetComponents<Component>()!.Where(ComponentFilter).OrderBy(o => o.GetType().Name).ToArray();
        var children   = ShowChildren(gameObject) ? EnumerateChildren().Where(ChildFilter).OrderBy(o => o.name).ToArray() : [];

        builder.AddSection(gameObject.name!, section => {
            if (!_SelectedChild.ContainsKey(level + 2)) {
                BuildFieldsForGameObject(section, gameObject, components, children, level);
            }

            if (_SelectedChild[level] > 0) {
                BuildSectionForGameObject(section, children[_SelectedChild[level] - 1], level + 1);
            }
        });

        return;

        IEnumerable<GameObject> EnumerateChildren() {
            for (var i = 0; i < gameObject.transform.childCount; i++) {
                yield return gameObject.transform.GetChild(i)!.gameObject;
            }
        }
    }

    private void BuildFieldsForGameObject(UIPanelBuilder builder, GameObject gameObject, Component[] components, GameObject[] children, int level) {
        builder.AddField("Position", builder.HStack(stack => {
            stack.AddInputField($"{gameObject.transform.localPosition}", _ => { }).FlexibleWidth();
            stack.Spacer();
            stack.AddButton("Show", Show(gameObject));
        }));
        builder.AddField("Path", builder.AddInputField($"{GetPath(gameObject)}", _ => { }));

        if (components.Length > 0) {
            List<string> componentNames = components.Select(o => o.GetType().Name).ToList();
            builder.AddField("Components", builder.AddDropdown(componentNames, _SelectedComponent[level], o => {
                _SelectedComponent[level] = o;
                Messenger.Default.Send(new RebuildSceneViewDialog());
            }));

            BuildSectionForComponent(builder, components[_SelectedComponent[level]]);
        }

        if (children.Length > 0) {
            List<string> childrenNames = ["Select...", ..children.Select(o => o.name)];
            builder.AddField("Children", builder.AddDropdown(childrenNames, _SelectedChild[level], o => {
                UpdateSelectedChild(level, o, children);
                Messenger.Default.Send(new RebuildSceneViewDialog());
            }));
        }
    }

    private static Action Show(GameObject gameObject) => () => CameraSelector.shared.ZoomToPoint(gameObject.transform.position.WorldToGame());

    private static string GetPath(GameObject gameObject) {
        var names     = new Stack<string>();

        var transform = gameObject.transform;
        while (transform.parent != null!) {
            names.Push(transform.name);
            transform = transform.parent;
        }

        names.Push(transform.name);

        return string.Join("/", names);
    }

    // Transform component is shown elsewhere
    private static bool ComponentFilter(Component o) => o is not Transform;

    private static bool ShowChildren(GameObject o) =>
        o.GetComponent<SceneryAssetInstance>() == null &&
        o.GetComponent<Graph>() == null;

    private static bool ChildFilter(GameObject o) => !o.name!.StartsWith("Cube.");

    private void UpdateSelectedChild(int level, int selectedChild, GameObject[] children) {
        _SelectedChild[level] = selectedChild;

        var trash = _SelectedChild.Keys.Where(o => o > level).ToArray();
        foreach (var i in trash) {
            _SelectedComponent.Remove(i);
            _SelectedChild.Remove(i);
        }

        if (selectedChild > 0) {
            _SelectedChild[level + 1] = 0;
            _SelectedComponent[level + 1] = 0;
        }
    }

    private record RebuildSceneViewDialog;

    #region BuildSectionForComponent

    private static void BuildSectionForComponent(UIPanelBuilder builder, Component component) {
        switch (component) {
            case SceneryAssetInstance sceneryAssetInstance:
                BuildSectionForSceneryAssetInstance(builder, sceneryAssetInstance);
                break;
            case Graph graph:
                BuildSectionForGraph(builder, graph);
                break;
        }
    }

    private static void BuildSectionForSceneryAssetInstance(UIPanelBuilder builder, SceneryAssetInstance sceneryAssetInstance) {
        builder.AddField("Identifier", sceneryAssetInstance.identifier!);
    }

    private static void BuildSectionForGraph(UIPanelBuilder builder, Graph graph) {
        builder.AddField("", builder.AddLabel("Graph contains all track nodes, segments, spans, etc."));
    }

    #endregion
}
