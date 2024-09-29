using Helpers;
using JetBrains.Annotations;
using UnityEngine;

namespace MapEditor.Features.TelegraphPoleEditor.Visualizers;

[PublicAPI]
internal sealed class TelegraphPoleVisualizer : MonoBehaviour, IPickable
{
    #region IPickable

    public void Activate(PickableActivateEvent evt) {
        MapEditorPlugin.UpdateState(state => state with { SelectedAsset = new TelegraphPoleId(NodeId) });
    }

    public void Deactivate() {
    }

    public float                    MaxPickDistance  => MapEditorPlugin.State.SelectedPatch != null ? 200f : 0f;
    public int                      Priority         => 1;
    public TooltipInfo              TooltipInfo      => new("Telegraph Pole", $"Position: {transform.localPosition}");
    public PickableActivationFilter ActivationFilter => PickableActivationFilter.PrimaryOnly;

    #endregion

    private static readonly Material _LineMaterial = new(Shader.Find("Universal Render Pipeline/Lit")!);

    private LineRenderer _LineRenderer = null!;
    public  int          NodeId;

    public void Awake() {
        gameObject.layer = Layers.Clickable;

        var boxCollider = gameObject.AddComponent<BoxCollider>();
        boxCollider.center = new Vector3(0, 8.9f, 0);
        boxCollider.size = Vector3.one;

        _LineRenderer = CreateLineRenderer();
    }

    public void Update() {
        _LineRenderer.enabled = MapEditorPlugin.State.TelegraphPole?.Id == NodeId;
    }

    private LineRenderer CreateLineRenderer() {
        var lineRenderer = gameObject.AddComponent<LineRenderer>();
        lineRenderer.material = _LineMaterial;
        lineRenderer.material.color = Color.yellow;
        lineRenderer.startWidth = 0.05f;
        lineRenderer.positionCount = 5;
        lineRenderer.useWorldSpace = false;
        lineRenderer.SetPosition(0, new Vector3(-0.2f, 10.5f, 0));
        lineRenderer.SetPosition(1, new Vector3(0, 10, 0));
        lineRenderer.SetPosition(2, new Vector3(0, 10 + 5f, 0));
        lineRenderer.SetPosition(3, new Vector3(0, 10, 0));
        lineRenderer.SetPosition(4, new Vector3(0.2f, 10.5f, 0));
        lineRenderer.enabled = true;
        return lineRenderer;
    }
}
