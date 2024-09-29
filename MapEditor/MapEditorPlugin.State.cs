using System;
using GalaSoft.MvvmLight.Messaging;
using MapEditor.Events;
using MapEditor.Features.SceneryAssetEditor;
using MapEditor.Features.TelegraphPoleEditor;
using MapEditor.Features.TrackNodeEditor;
using MapEditor.Features.TrackNodeEditor.Visualizer;
using MapEditor.Features.TrackSegmentEditor;
using MapEditor.Features.TrackSegmentEditor.Visualizer;
using MapEditor.Utility;
using StrangeCustoms.Tracks;
using TriangleNet.Logging;

namespace MapEditor;

public sealed partial class MapEditorPlugin
{
    public static PatchEditor?   PatchEditor { get; private set; }
    public static MapEditorState State       { get; private set; } = new();

    public static void ResetState() => UpdateState(_ => new MapEditorState());

    public static void UpdateState(Func<MapEditorState, bool> predicate, UpdateDelegate<MapEditorState> update) {
        if (predicate(State)) {
            UpdateState(update);
        }
    }

    public static void UpdateState(UpdateDelegate<MapEditorState> update) {
        var newState = update(State);
        if (State == newState) {
            return;
        }

        var oldState = State;
        State = newState;
        UnityHelpers.CallOnNextFrame(() => Shared!.OnMapEditorStateChanged(oldState));
        Messenger.Default.Send(new MapEditorStateChanged());
    }

    private void OnMapEditorStateChanged(MapEditorState oldState) {
#if DEBUG
        global::UI.Console.Console.shared.AddLine($"State: {State}");
#endif
        if (oldState.SelectedPatch != State.SelectedPatch) {
            if (State.SelectedPatch != null) {
                PatchEditor = new PatchEditor(State.SelectedPatch);
                TrackNodeVisualizerManager.CreateVisualizers();
            } else {
                PatchEditor = null;
                TrackNodeVisualizerManager.DestroyVisualizers();
            }
        }

        if (State.TrackNode != null) {
            TrackNodeDialog.Show(_UiHelper, State.TrackNode);
            TrackSegmentVisualizerManager.CreateVisualizersForNode(State.TrackNode);
        } else {
            TrackNodeDialog.Close();
            TrackSegmentVisualizerManager.DestroyVisualizers();
        }

        if (State.TrackSegment != null) {
            TrackSegmentDialog.Show(_UiHelper, State.TrackSegment);
        } else {
            TrackSegmentDialog.Close();
        }

        if (State.TelegraphPole != null) {
            TelegraphPoleDialog.Show(_UiHelper);
        } else {
            TelegraphPoleDialog.Close();
        }

        if (State.SceneryAssetInstance != null) {
            SceneryAssetDialog.Show(_UiHelper);
        } else {
            SceneryAssetDialog.Close();
        }
    }
}

public delegate T UpdateDelegate<T>(T oldValue);
