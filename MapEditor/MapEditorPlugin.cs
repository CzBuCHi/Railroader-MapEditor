using System;
using GalaSoft.MvvmLight.Messaging;
using Game.Events;
using JetBrains.Annotations;
using MapEditor.Features.Editor;
using MapEditor.TopRightArea;
using MapEditor.Utility;
using Railloader;

namespace MapEditor;

[PublicAPI]
public sealed partial class MapEditorPlugin(IModdingContext context, IUIHelper uiHelper) : SingletonPluginBase<MapEditorPlugin>
{
    private const string PluginIdentifier = "CzBuCHi.MapEditor";

    private readonly IModdingContext _ModdingContext = context;
    private readonly IUIHelper       _UiHelper       = uiHelper;

    private static IModdingContext ModdingContext => Shared!._ModdingContext;

    public override void OnEnable() {
        var harmony = new HarmonyLib.Harmony(PluginIdentifier);
        harmony.PatchAll();

        Messenger.Default.Register(this, new Action<MapDidLoadEvent>(OnMapDidLoad));
        Messenger.Default.Register(this, new Action<MapDidUnloadEvent>(OnMapDidUnload));
    }

    public override void OnDisable() {
        var harmony = new HarmonyLib.Harmony(PluginIdentifier);
        harmony.UnpatchAll();

        Messenger.Default.Unregister(this);
    }

    private void OnMapDidLoad(MapDidLoadEvent @event) {
        TopRightAreaExtension.AddButton(OpenEditorDialog);
        KeyboardTransform.Initialize();
        UnityHelpers.Initialize();
    }

    private void OnMapDidUnload(MapDidUnloadEvent @event) {
        PatchEditor = null;
        _Settings = null;
        _Listener = null;
        KeyboardTransform.Destroy();
    }

    private void OpenEditorDialog() {
        EditorDialog.Show(_ModdingContext, _UiHelper);
    }
}
