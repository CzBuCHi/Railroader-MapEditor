using MapEditor.Utility;
using Track;
using UnityEngine;

namespace MapEditor.Features.TrackSegmentEditor.Visualizer;

public static class TrackSegmentVisualizerManager
{
    public static void CreateVisualizersForNode(TrackNode node) {
        DestroyVisualizers();
        var segments = Graph.Shared.SegmentsConnectedTo(node);
        foreach (var segment in segments) {
            CreateTrackSegmentVisualizer(segment);
        }
    }

    public static void CreateTrackSegmentVisualizer(TrackSegment trackSegment, bool showChevrons = true, float start = 0, float end = 1) {
        UnityHelpers.CreateGameObject("TrackSegmentVisualizer_" + trackSegment.id, go => {
            go.transform.parent = trackSegment.transform;
            var visualizer = go.AddComponent<TrackSegmentVisualizer>();
            visualizer.ShowChevrons = showChevrons;
            visualizer.StartPoint = start;
            visualizer.EndPoint = end;
        });
    }

    public static void DestroyVisualizers() {
        var visualizers = Graph.Shared.GetComponentsInChildren<TrackSegmentVisualizer>()!;
        foreach (var visualizer in visualizers) {
            Object.Destroy(visualizer.gameObject);
        }
    }
}
