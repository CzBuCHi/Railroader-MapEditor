using System.Collections.Generic;
using System.IO;
using System.Linq;
using Helpers;
using MapEditor.Events;
using MapEditor.Features.Abstract;
using MapEditor.Features.Editor.Harmony;
using MapEditor.Utility;
using Railloader;
using Track;
using UI.Builder;
using UI.Common;
using UnityEngine;

namespace MapEditor.Features.Editor;

public sealed class EditorDialog(IModdingContext context, IUIHelper uiHelper) : DialogBase(uiHelper)
{
    #region Manage

    private static EditorDialog? _Instance;

    public static void Show(IModdingContext context, IUIHelper uiHelper) {
        Show(ref _Instance, () => new EditorDialog(context, uiHelper));
    }

    public static void Close() {
        Close(ref _Instance);
    }

    #endregion

    protected override int             WindowWidth    => 400;
    protected override int             WindowHeight   => 250;
    protected override Window.Position WindowPosition => Window.Position.UpperRight;
    protected override string          WindowTitle    => "Map Editor";

    private readonly Dictionary<string, List<string>> _Graphs =
        context.GetMixintos("game-graph")
               .GroupBy(o => o.Source.ToString(), o => o.Mixinto)
               .ToDictionary(o => o.Key, o => o.ToList());

    private List<string>? _Mods;
    private List<string>  Mods => _Mods ??= ["Select ...", .._Graphs.Keys];

    protected override void ConfigureWindow(Window window) {
        base.ConfigureWindow(window);
        var rectTransform = window.GetComponent<RectTransform>()!;
        rectTransform.position = new Vector2(Screen.width, Screen.height - 50).Round();
        window.ClampToParentBounds();
    }

    protected override void OnWindowClosed() {
        base.OnWindowClosed();
        MapEditorPlugin.ResetState();
    }

    private int _ModIndex;
    private int _GraphIndex;

    protected override void BuildWindow(UIPanelBuilder builder) {
        builder
            .AddField("Mod",
                builder.AddDropdown(Mods, _ModIndex, o => {
                    _ModIndex = o;
                    _GraphIndex = 0;
                    MapEditorStateStepManager.UndoAll();
                    MapEditorStateStepManager.Clear();
                    if (_ModIndex == 0) {
                        UpdateState(null);
                    }

                    builder.Rebuild();
                })!
            )!
            .Disable(MapEditorStateStepManager.Count > 0);

        if (_ModIndex == 0) {
            builder.AddExpandingVerticalSpacer();
            return;
        }

        var modGraphs = _Graphs[Mods[_ModIndex]]!;
        if (modGraphs.Count > 1) {
            builder
                .AddField("Graph",
                    builder.AddDropdown(modGraphs.Select(Path.GetFileNameWithoutExtension).ToList(), _GraphIndex, o => {
                        _GraphIndex = o;
                        MapEditorStateStepManager.UndoAll();
                        MapEditorStateStepManager.Clear();
                        UpdateState(modGraphs[_GraphIndex]);
                    })!
                )!
                .Disable(MapEditorStateStepManager.Count > 0);
        } else {
            _GraphIndex = 0;
            UpdateState(modGraphs[_GraphIndex]);
        }

        if (MapEditorPlugin.State.SelectedPatch != null) {
            builder.AddSection("", BuildEditor);
        }

        builder.AddExpandingVerticalSpacer();

        return;

        void UpdateState(string? selectedPatch) => MapEditorPlugin.UpdateState(state => state with {
            SelectedPatch = selectedPatch,
            SelectedAsset = null,
            ShowSpans = state.ShowSpans && selectedPatch != null
        });
    }

    private static void BuildEditor(UIPanelBuilder builder) {
        builder.RebuildOnEvent<MapEditorStateChanged>();

        builder.AddField("Prefix", builder.AddInputField(IdGenerators.Prefix, s => IdGenerators.Prefix = s)!);

        builder.AddField("Changes", MapEditorStateStepManager.Steps, UIPanelBuilder.Frequency.Periodic);

        builder.ButtonStrip(strip => {
            strip.RebuildOnEvent<MapEditorTransformChanged>();
            strip.AddButton("Undo All", MapEditorStateStepManager.UndoAll).Disable(!MapEditorStateStepManager.CanUndo);
            strip.AddButton("Undo", MapEditorStateStepManager.Undo).Disable(!MapEditorStateStepManager.CanUndo);
            strip.AddButton("Redo", MapEditorStateStepManager.Redo).Disable(!MapEditorStateStepManager.CanRedo);
            strip.AddButton("Redo All", MapEditorStateStepManager.RedoAll).Disable(!MapEditorStateStepManager.CanRedo);
        });

        builder.AddField("Show spans",
            builder.AddToggle(() => MapEditorPlugin.State.ShowSpans, o => MapEditorPlugin.UpdateState(state => state with { ShowSpans = o }))!
        );

        builder.ButtonStrip(strip => {
            strip.AddButton("Rebuild Track", TrackObjectManager.Instance.Rebuild);
            strip.AddButton("Save changes", () => {
                MapEditorPlugin.PatchEditor!.Save();
                MapEditorStateStepManager.Clear();
            });
        });
    }
}
