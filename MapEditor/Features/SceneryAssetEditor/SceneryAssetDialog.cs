using MapEditor.Events;
using MapEditor.Extensions;
using MapEditor.Features.Abstract;
using Railloader;
using UI.Builder;
using UI.Common;

namespace MapEditor.Features.SceneryAssetEditor;

public sealed class SceneryAssetDialog(IUIHelper uiHelper) : DialogBase(uiHelper)
{
    #region Manage

    private static SceneryAssetDialog? _Instance;

    public static void Show(IUIHelper uiHelper) {
        Show(ref _Instance, () => new SceneryAssetDialog(uiHelper));
    }

    public static void Close() {
        Close(ref _Instance);
    }

    #endregion

    protected override int             WindowWidth    => 400;
    protected override int             WindowHeight   => 300;
    protected override Window.Position WindowPosition => Window.Position.LowerRight;
    protected override string          WindowTitle    => "Map Editor | Scenery asset";

    protected override void OnWindowClosed() {
        base.OnWindowClosed();
        MapEditorPlugin.UpdateState(state => state with { SelectedAsset = null });
    }

    protected override void BuildWindow(UIPanelBuilder builder) {
        builder.RebuildOnEvent<MapEditorStateChanged>();
        builder.RebuildOnEvent<MapEditorTransformChanged>();

        var asset = MapEditorPlugin.State.SceneryAssetInstance!;

        builder.AddField("Name", builder.AddInputField($"{asset.name}", _ => { })!);
        builder.AddField("Position", builder.AddInputField(asset.transform.localPosition.ToString(), _ => { })!);
        builder.AddField("Rotation", builder.AddInputField(asset.transform.localEulerAngles.ToString(), _ => { })!);
        builder.AddField("Identifier",
            builder.AddDropdown(SceneryAssetUtility.Identifiers, SceneryAssetUtility.Identifiers.IndexOf(asset.identifier), o => {
                asset.identifier = SceneryAssetUtility.Identifiers[o]!;
            })!
        );

        builder.AddField("Transform mode",
            builder.ButtonStrip(strip => {
                strip.AddButtonSelectable("Move", MapEditorPlugin.State.TransformMode == TransformMode.Move, () => MapEditorPlugin.UpdateState(o => o with { TransformMode = TransformMode.Move }));
                strip.AddButtonSelectable("Rotate", MapEditorPlugin.State.TransformMode == TransformMode.Rotate, () => MapEditorPlugin.UpdateState(o => o with { TransformMode = TransformMode.Rotate }));
            })!
        );

        builder.AddExpandingVerticalSpacer();
    }
}
