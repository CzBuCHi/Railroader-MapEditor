using Helpers;
using Serilog;
using UnityEngine;

namespace MapEditor.Features.SceneryAssetEditor.Visualizers;

public static class SceneryAssetVisualizerManager
{
    public static void CreateVisualizers() {
        foreach (var asset in Object.FindObjectsOfType<SceneryAssetInstance>()!) {
            CreateVisualizer(asset);
        }
    }

    public static void CreateVisualizer(SceneryAssetInstance asset) {
        if (asset.GetComponentInChildren<SceneryAssetVisualizer>() != null) {
            return;
        }
        
        var go = new GameObject("SceneryAssetVisualizer");
        go.transform.SetParent(asset.transform);
        go.transform.localPosition = Vector3.zero;
        go.transform.localEulerAngles = Vector3.zero;
        go.AddComponent<SceneryAssetVisualizer>();
    }
}
