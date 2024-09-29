using MapEditor.Events;
using MapEditor.Extensions;
using MapEditor.Features.Abstract;
using Railloader;
using Track;
using UI.Builder;
using UI.Common;

namespace MapEditor.Features.TrackSegmentEditor;

using static Window;

public sealed class TrackSegmentDialog(IUIHelper uiHelper, TrackSegment trackSegment) : DialogBase(uiHelper)
{
    private TrackSegment _TrackSegment = trackSegment;

    #region Manage

    private static TrackSegmentDialog? _Instance;

    public static void Show(IUIHelper uiHelper, TrackSegment trackSegment) {
        Show(ref _Instance, () => new TrackSegmentDialog(uiHelper, trackSegment), o => o._TrackSegment = trackSegment);
    }

    public static void Close() {
        Close(ref _Instance);
    }

    #endregion

    protected override int      WindowWidth    => 400;
    protected override int      WindowHeight   => 300;
    protected override Position WindowPosition => Position.LowerRight;
    protected override string   WindowTitle    => $"Map Editor | Segment '{MapEditorPlugin.State.TrackSegment!.id}'";

    protected override void OnWindowClosed() {
        MapEditorPlugin.UpdateState(state => state with { SelectedAsset = null });
    }

    private string?                           _GroupId;
    private int?                              _Priority;
    private int?                              _SpeedLimit;
    private TrackClass?                       _TrackClass;
    private TrackSegment.Style?               _Style;
    private AutoTrestle.AutoTrestle.EndStyle _TrestleHead;
    private AutoTrestle.AutoTrestle.EndStyle _TrestleTail;

    protected override void BuildWindow(UIPanelBuilder builder) {
        builder.RebuildOnEvent<MapEditorStateChanged>();

        builder.AddField("Id", builder.AddInputField(_TrackSegment.id, _ => { })!)!.Disable(true);

        builder.AddField("Group ID", builder.AddInputField(_GroupId ?? _TrackSegment.groupId ?? "", o => _GroupId = o)!);
        builder.AddField("Priority", builder.AddSliderQuantized(() => _Priority ?? _TrackSegment.priority,
                () => (_Priority ?? _TrackSegment.priority).ToString("0"),
                o => _Priority = (int)o, 1, -2, 2,
                o => _Priority = (int)o)!
        );

        builder.AddField("Speed Limit", builder.AddSliderQuantized(() => _SpeedLimit ?? _TrackSegment.speedLimit,
                () => (_SpeedLimit ?? _TrackSegment.speedLimit).ToString("0"),
                o => _SpeedLimit = (int)o * 5, 1, 0, 9,
                o => _SpeedLimit = (int)o * 5)!
        );
        builder.AddField("Track Class", builder.AddEnumDropdown(_TrackClass ?? _TrackSegment.trackClass, o => _TrackClass = o));
        builder.AddField("Track Style", builder.AddEnumDropdown(_Style ?? _TrackSegment.style, UpdateStyle));
        if ((_Style ?? _TrackSegment.style) == TrackSegment.Style.Bridge) {
            builder.AddField("Trestle Head Style", builder.AddEnumDropdown(_TrestleHead, o => _TrestleHead = o));
            builder.AddField("Trestle Tail Style", builder.AddEnumDropdown(_TrestleTail, o => _TrestleTail = o));
        }

        builder.AddSection("Operations", section => {
            section.ButtonStrip(strip => {
                strip.AddButton("Update properties", TrackSegmentUtility.UpdateSegment(_GroupId, _Priority, _SpeedLimit, _TrackClass, _Style, _TrestleHead, _TrestleTail));
                strip.AddButton("Remove", TrackSegmentUtility.Remove());
                strip.AddButton("Inject Node", TrackSegmentUtility.InjectNode());
            });
        });

        builder.AddExpandingVerticalSpacer();
        return;

        void UpdateStyle(TrackSegment.Style style) {
            if (_Style == style) {
                return;
            }

            _Style = style;
            builder.Rebuild();
        }
    }
}
