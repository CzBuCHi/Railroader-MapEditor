using System;
using GalaSoft.MvvmLight.Messaging;
using Helpers;
using MapEditor.Events;
using MapEditor.Features.SceneryAssetEditor;
using MapEditor.Features.TelegraphPoleEditor;
using MapEditor.Features.TrackNodeEditor;
using MapEditor.Features.TrackNodeEditor.Visualizer;
using MapEditor.Features.TrackSegmentEditor;
using MapEditor.Features.TrackSegmentEditor.Visualizer;
using MapEditor.Utility;
using Serilog;
using StrangeCustoms.Tracks;
using TelegraphPoles;
using Track;

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
        UI.Console.Console.shared.AddLine($"State: {State}");
        Log.Information($"State: {State}");
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

        if (oldState.SelectedAsset != State.SelectedAsset) {
            OnMapEditorStateSelectedAssetChanged(oldState.SelectedAsset, State.SelectedAsset);
        }
    }

    private void OnMapEditorStateSelectedAssetChanged(object? oldSelectedAsset, object? newSelectedAsset) {
        switch (oldSelectedAsset) {
            case TrackNode:
                TrackNodeDialog.Close();
                TrackSegmentVisualizerManager.DestroyVisualizers();
                break;

            case TrackSegment:
                TrackSegmentDialog.Close();
                TrackSegmentVisualizerManager.DestroyVisualizers();
                break;

            case TelegraphPoleId:
                TelegraphPoleDialog.Close();
                break;

            case SceneryAssetInstance:
                SceneryAssetDialog.Close();
                break;
        }

        switch (newSelectedAsset) {
            case TrackNode trackNode:
                TrackNodeDialog.Show(_UiHelper, trackNode);
                TrackSegmentVisualizerManager.CreateVisualizersForNode(trackNode);
                break;

            case TrackSegment trackSegment:
                TrackSegmentDialog.Show(_UiHelper, trackSegment);
                TrackSegmentVisualizerManager.CreateTrackSegmentVisualizer(trackSegment);
                break;

            case TelegraphPoleId:
                TelegraphPoleDialog.Show(_UiHelper);
                break;

            case SceneryAssetInstance:
                SceneryAssetDialog.Show(_UiHelper);
                break;
        }
    }
}

public delegate T UpdateDelegate<T>(T oldValue);
