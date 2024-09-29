using System.Text;
using Helpers;
using JetBrains.Annotations;
using Serilog;
using Track;
using UnityEngine;

namespace MapEditor.Features.SceneryAssetEditor.Visualizers;

[PublicAPI]
internal sealed class SceneryAssetVisualizer : MonoBehaviour , IPickable
{
    #region IPickable

    public void Activate(PickableActivateEvent evt) {
        MapEditorPlugin.UpdateState(state => state with { SelectedAsset = _SceneryAssetInstance });
    }

    public void Deactivate() {
    }

    public float                    MaxPickDistance  => MapEditorPlugin.State.SelectedPatch != null ? 200f : 0f;
    public int                      Priority         => 1;
    public TooltipInfo              TooltipInfo      => MapEditorPlugin.State.SelectedPatch != null ? BuildTooltipInfo() : TooltipInfo.Empty;
    public PickableActivationFilter ActivationFilter => PickableActivationFilter.PrimaryOnly;

    #endregion

    private TooltipInfo BuildTooltipInfo() {
        var sb = new StringBuilder();
        sb.AppendLine($"ID: {_SceneryAssetInstance.name}");
        sb.AppendLine($"Pos: {_SceneryAssetInstance.transform.localPosition}");
        sb.AppendLine($"Rot: {_SceneryAssetInstance.transform.localEulerAngles}");
        sb.AppendLine($"Identifier: {_SceneryAssetInstance.identifier}");
        return new TooltipInfo($"Scenery {_SceneryAssetInstance.name}", sb.ToString());
    }


    private static readonly Material _LineMaterial = new(Shader.Find("Universal Render Pipeline/Lit")!);

    private LineRenderer _LineRenderer = null!;

    private SceneryAssetInstance _SceneryAssetInstance = null!;


    public void Awake() {
        Log.Information("SceneryAssetVisualizer: Awake");

        _SceneryAssetInstance = gameObject.GetComponentInParent<SceneryAssetInstance>()!;
        gameObject.layer = Layers.Clickable;

        var boxCollider = gameObject.AddComponent<BoxCollider>();
        boxCollider.center = Vector3.zero;
        boxCollider.size = Vector3.one * 5;

        _LineRenderer = CreateLineRenderer();
    }

    public void Update() {
        _LineRenderer.enabled = MapEditorPlugin.State.SceneryAssetInstance == _SceneryAssetInstance;
    }

    private LineRenderer CreateLineRenderer() {
        var lineRenderer = gameObject.AddComponent<LineRenderer>();
        lineRenderer.material = _LineMaterial;
        lineRenderer.material.color = Color.yellow;
        lineRenderer.startWidth = 0.05f;

        lineRenderer.useWorldSpace = false;

        var cubeVertices = GetCubeVertices(Vector3.zero, Vector3.one * 10);

        var cubeEdges = new[] {
            0, 1, 1, 2, 2, 3, 3, 0, // Bottom square
            4, 5, 5, 6, 6, 7, 7, 4, // Top square
            0, 4, 1, 5, 2, 6, 3, 7  // Vertical connections
        };

        lineRenderer.positionCount = cubeEdges.Length;
        for (var i = 0; i < cubeEdges.Length; i++) {
            lineRenderer.SetPosition(i, cubeVertices[cubeEdges[i]]);
        }

        lineRenderer.enabled = true;
        return lineRenderer;
    }

    private Vector3[] GetCubeVertices(Vector3 center, Vector3 size) {
        var half = size / 2;
        return new Vector3[] {
            new(center.x - half.x, center.y - half.y, center.z - half.z), // Bottom 4 vertices
            new(center.x + half.x, center.y - half.y, center.z - half.z),
            new(center.x + half.x, center.y - half.y, center.z + half.z),
            new(center.x - half.x, center.y - half.y, center.z + half.z),

            new(center.x - half.x, center.y + half.y, center.z - half.z), // Top 4 vertices
            new(center.x + half.x, center.y + half.y, center.z - half.z),
            new(center.x + half.x, center.y + half.y, center.z + half.z),
            new(center.x - half.x, center.y + half.y, center.z + half.z)
        };
    }
}
