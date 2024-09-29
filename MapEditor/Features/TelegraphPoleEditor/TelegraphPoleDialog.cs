using MapEditor.Events;
using MapEditor.Extensions;
using MapEditor.Features.Abstract;
using Railloader;
using UI.Builder;
using UI.Common;

namespace MapEditor.Features.TelegraphPoleEditor;

public sealed class TelegraphPoleDialog(IUIHelper uiHelper) : DialogBase(uiHelper)
{
    #region Manage

    private static TelegraphPoleDialog? _Instance;

    public static void Show(IUIHelper uiHelper) {
        Show(ref _Instance, () => new TelegraphPoleDialog(uiHelper));
    }

    public static void Close() {
        Close(ref _Instance);
    }

    #endregion

    protected override int             WindowWidth    => 400;
    protected override int             WindowHeight   => 300;
    protected override Window.Position WindowPosition => Window.Position.LowerRight;
    protected override string          WindowTitle    => "Map Editor | Telegraph Pole";

    protected override void OnWindowClosed() {
        MapEditorPlugin.UpdateState(state => state.TelegraphPole != null, state => state with { SelectedAsset = null });
    }

    protected override void BuildWindow(UIPanelBuilder builder) {
        builder.RebuildOnEvent<MapEditorStateChanged>();
        builder.RebuildOnEvent<MapEditorTransformChanged>();

        var nodeId = MapEditorPlugin.State.TelegraphPole!;
        var pole = TelegraphPoleUtility.GetTelegraphPole(nodeId.Id);

        builder.AddField("Id", builder.AddInputField($"{nodeId.Id}", _ => { })!);
        builder.AddField("Position", builder.AddInputField(pole.transform.localPosition.ToString(), _ => { })!);
        builder.AddField("Rotation", builder.AddInputField(pole.transform.localEulerAngles.ToString(), _ => { })!);

        builder.AddField("Transform mode",
            builder.ButtonStrip(strip => {
                strip.AddButtonSelectable("Move", MapEditorPlugin.State.TransformMode == TransformMode.Move, () => MapEditorPlugin.UpdateState(o => o with { TransformMode = TransformMode.Move }));
                strip.AddButtonSelectable("Rotate", MapEditorPlugin.State.TransformMode == TransformMode.Rotate, () => MapEditorPlugin.UpdateState(o => o with { TransformMode = TransformMode.Rotate }));
            })!
        );

        builder.AddExpandingVerticalSpacer();
    }
}
