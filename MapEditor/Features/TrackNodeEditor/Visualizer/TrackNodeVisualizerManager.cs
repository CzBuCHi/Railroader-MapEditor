using Track;
using UnityEngine;

namespace MapEditor.Features.TrackNodeEditor.Visualizer;

internal static class TrackNodeVisualizerManager
{
    public static void CreateVisualizers() {
        foreach (var trackNode in Graph.Shared.Nodes) {
            CreateVisualizer(trackNode);
        }
    }

    public static void DestroyVisualizers() {
        foreach (var visualizer in Graph.Shared.GetComponentsInChildren<TrackNodeVisualizer>()!) {
            Object.Destroy(visualizer.gameObject);
        }
    }

    public static void CreateVisualizer(TrackNode node) {
        if (node.GetComponentInChildren<TrackNodeVisualizer>() != null) {
            return;
        }

        var go = new GameObject("TrackNodeVisualizer");
        go.transform.SetParent(node.transform);
        go.AddComponent<TrackNodeVisualizer>();
    }
}
