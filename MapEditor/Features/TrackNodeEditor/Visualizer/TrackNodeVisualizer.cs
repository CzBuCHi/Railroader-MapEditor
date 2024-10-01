using System.Text;
using Helpers;
using JetBrains.Annotations;
using Track;
using UnityEngine;

namespace MapEditor.Features.TrackNodeEditor.Visualizer;

[PublicAPI]
internal sealed class TrackNodeVisualizer : MonoBehaviour, IPickable
{
    private static readonly Material _LineMaterial = new(Shader.Find("Universal Render Pipeline/Lit")!);

    private TrackNode _TrackNode = null!;

    private LineRenderer? _LineRenderer;

    private TooltipInfo BuildTooltipInfo() {
        var sb = new StringBuilder();
        sb.AppendLine($"ID: {_TrackNode.id}");
        sb.AppendLine($"Pos: {_TrackNode.transform.localPosition}");
        sb.AppendLine($"Rot: {_TrackNode.transform.localEulerAngles}");
        return new TooltipInfo($"Node {_TrackNode.id}", sb.ToString());
    }

    public void Start() {
        _TrackNode = transform.parent.GetComponent<TrackNode>()!;

        transform.localPosition = Vector3.zero;
        transform.localEulerAngles = Vector3.zero;

        gameObject.layer = Layers.Clickable;

        _LineRenderer = gameObject.AddComponent<LineRenderer>();
        _LineRenderer.material = _LineMaterial;
        _LineRenderer.startWidth = 0.05f;
        _LineRenderer.positionCount = 5;
        _LineRenderer.useWorldSpace = false;

        const float sizeX = -0.2f;
        const float sizeY = -0.4f;
        const float sizeZ = 0.3f;

        _LineRenderer.SetPosition(0, new Vector3(-sizeX, 0, sizeY));
        _LineRenderer.SetPosition(1, new Vector3(0, 0, sizeZ));
        _LineRenderer.SetPosition(2, new Vector3(sizeX, 0, sizeY));
        _LineRenderer.SetPosition(3, new Vector3(0, 0, -sizeZ));
        _LineRenderer.SetPosition(4, new Vector3(-sizeX, 0, sizeY));

        var boxCollider = gameObject.AddComponent<BoxCollider>();
        boxCollider.size = new Vector3(0.4f, 0.4f, 0.8f);
    }

    public void Update() {
        _LineRenderer!.material.color = MapEditorPlugin.State.TrackNode == _TrackNode ? Color.magenta : Color.cyan;
    }

    #region IPickable

    public void Activate(PickableActivateEvent evt) {
        var connectToPrevious = Input.GetKey(KeyCode.LeftShift) || Input.GetKey(KeyCode.RightShift);
        if (connectToPrevious) {
            TrackNodeUtility.TryConnectToCurrent(_TrackNode);
        } else {
            MapEditorPlugin.UpdateState(state => state with { SelectedAsset = _TrackNode });
        }
    }

    public void Deactivate() {
    }

    public float                    MaxPickDistance  => MapEditorPlugin.State.SelectedPatch != null ? 200f : 0f;
    public int                      Priority         => 1;
    public TooltipInfo              TooltipInfo      => MapEditorPlugin.State.SelectedPatch != null ? BuildTooltipInfo() : TooltipInfo.Empty;
    public PickableActivationFilter ActivationFilter => PickableActivationFilter.PrimaryOnly;

    #endregion
}
