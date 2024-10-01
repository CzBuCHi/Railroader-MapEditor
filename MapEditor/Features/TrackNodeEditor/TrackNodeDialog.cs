using MapEditor.Events;
using MapEditor.Features.Abstract;
using MapEditor.Features.TrackNodeEditor.StateSteps;
using MapEditor.Utility;
using Railloader;
using Serilog;
using Track;
using UI.Builder;
using UI.Common;

namespace MapEditor.Features.TrackNodeEditor;

using static Window;

public sealed class TrackNodeDialog(IUIHelper uiHelper, TrackNode trackNode) : DialogBase(uiHelper)
{
    private TrackNode _TrackNode = trackNode;

    #region Manage

    private static TrackNodeDialog? _Instance;

    public static void Show(IUIHelper uiHelper, TrackNode trackNode) {
        Show(ref _Instance, () => new TrackNodeDialog(uiHelper, trackNode), o => o._TrackNode = trackNode);
    }

    public static void Close() {
        Close(ref _Instance);
    }

    #endregion

    protected override int      WindowWidth    => 400;
    protected override int      WindowHeight   => 300;
    protected override Position WindowPosition => Position.LowerRight;
    protected override string   WindowTitle    => $"Map Editor | Node: '{_TrackNode.id}'";

    protected override void OnWindowClosed() {
        MapEditorPlugin.UpdateState(state => state.TrackNode == _TrackNode, state => state with { SelectedAsset = null });
    }

    protected override void BuildWindow(UIPanelBuilder builder) {
        if (MapEditorPlugin.State.TrackNode == null || 
            MapEditorPlugin.State.TrackNode.id != _TrackNode.id) {
            return;
        }

        builder.RebuildOnEvent<MapEditorStateChanged>();

        builder.AddField("Id", builder.AddInputField(_TrackNode.id, _ => { })).Disable(true);
        builder.AddField("Position", builder.AddInputField(_TrackNode.transform.localPosition.ToString(), _ => { })).Disable(true);
        builder.AddField("Rotation", builder.AddInputField(_TrackNode.transform.localEulerAngles.ToString(), _ => { })).Disable(true);

        builder.AddField("Transform mode",
            builder.ButtonStrip(strip => {
                strip.AddButtonSelectable("Move", MapEditorPlugin.State.TransformMode == TransformMode.Move, () => MapEditorPlugin.UpdateState(o => o with { TransformMode = TransformMode.Move }));
                strip.AddButtonSelectable("Rotate", MapEditorPlugin.State.TransformMode == TransformMode.Rotate, () => MapEditorPlugin.UpdateState(o => o with { TransformMode = TransformMode.Rotate }));
            })
        );

        if (Graph.Shared.IsSwitch(_TrackNode)) {
            builder.AddField("Flip Switch Stand",
                builder.AddToggle(() => _TrackNode.flipSwitchStand, val => MapEditorStateStepManager.NextStep(new TrackNodeUpdate(_TrackNode.id) { FlipSwitchStand = val }))!
            );
        }

        builder.AddSection("Operations", section => {
            section.ButtonStrip(strip => {
                strip.AddButton("Show", TrackNodeUtility.Show);
                strip.AddButton("Remove", TrackNodeUtility.Remove);
                strip.AddButton("Create new", TrackNodeUtility.Add).Disable(Graph.Shared.IsSwitch(_TrackNode));
                strip.AddButton("Split", TrackNodeUtility.Split).Disable(Graph.Shared.NodeIsDeadEnd(_TrackNode, out _));
            });
        });

        builder.AddExpandingVerticalSpacer();
    }
}
