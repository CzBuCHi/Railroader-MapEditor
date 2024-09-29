using System.Text;
using Helpers;
using JetBrains.Annotations;
using Track;
using UnityEngine;

namespace MapEditor.Features.TrackSegmentEditor.Visualizer;

[PublicAPI]
public sealed class TrackSegmentVisualizer : MonoBehaviour, IPickable
{
    private static readonly Color         _Yellow       = new(1, 1, 0);
    private readonly        Material      _LineMaterial = new(Shader.Find("Universal Render Pipeline/Lit")!);
    private                 LineRenderer? _LineRenderer;
    private                 Chevrons?     _Chevrons;
    private                 TrackSegment  _TrackSegment = null!;
    private                 bool          _PendingRebuild;
    private                 float         _StartPoint;
    private                 float         _EndPoint = 1;

    public float StartPoint {
        get => _StartPoint;
        set {
            _StartPoint = value;
            _PendingRebuild = true;
        }
    }

    public float EndPoint {
        get => _EndPoint;
        set {
            _EndPoint = value;
            _PendingRebuild = true;
        }
    }

    public bool ShowChevrons = true;

    public void Awake() {
        transform.localPosition = -transform.parent.localPosition;
        transform.localEulerAngles = Vector3.zero;

        _TrackSegment = gameObject.GetComponentInParent<TrackSegment>()!;
        _LineRenderer = AddLineRenderer(gameObject);
    }

    public void Start() {
        RebuildBezier();

        if (ShowChevrons) {
            _Chevrons = CreateChevrons();
        }
    }

    public void Update() {
        if (_PendingRebuild) {
            RebuildBezier();
            _PendingRebuild = false;
        }

        if (ShowChevrons) {
            if (_Chevrons == null) {
                _Chevrons = CreateChevrons();
                RebuildChevrons();
            }
        } else {
            if (_Chevrons != null) {
                Destroy(_Chevrons.Start.gameObject);
                Destroy(_Chevrons.End.gameObject);
                _Chevrons = null;
            }
        }

        var color = MapEditorPlugin.State.TrackSegment == _TrackSegment ? Color.green : _Yellow;
        _LineMaterial.color = color;
    }

    public void RebuildBezier() {
        var positions = GetPoints();
        _LineRenderer!.positionCount = positions.Length;
        _LineRenderer.SetPositions(positions);
        RebuildChevrons();
    }

    private void RebuildChevrons() {
        if (_Chevrons == null) {
            return;
        }

        var length = _TrackSegment.GetLength();
        if (length < 15f) {
            UpdateChevron(_Chevrons.Start, 0.5f);
            _Chevrons.End.enabled = false;
        } else {
            var p = 5f / length;
            UpdateChevron(_Chevrons.Start, p);
            _Chevrons.End.enabled = true;
            UpdateChevron(_Chevrons.End, 1 - p);
        }

        return;

        void UpdateChevron(LineRenderer chevron, float p) {
            chevron.transform.localPosition = _TrackSegment.Curve.GetPoint(p) + new Vector3(0, 0.025f, 0);
            chevron.transform.localEulerAngles = _TrackSegment.Curve.GetRotation(p).eulerAngles;
        }
    }

    private Vector3[] GetPoints() {
        const int pointCount = 20;

        var points = new Vector3[pointCount];
        var px     = (EndPoint - StartPoint) / (pointCount - 1);
        var p      = StartPoint;
        for (var i = 0; i < pointCount; i++, p += px) {
            points[i] = _TrackSegment.Curve.GetPoint(p);
        }

        return points;
    }

    private LineRenderer AddLineRenderer(GameObject targetObject) {
        var lineRenderer = targetObject.AddComponent<LineRenderer>();
        lineRenderer.material = _LineMaterial;
        lineRenderer.startWidth = 0.05f;
        lineRenderer.endWidth = 0.05f;
        lineRenderer.useWorldSpace = false;
        return lineRenderer;
    }

    private Chevrons CreateChevrons() {
        return new Chevrons(CreateChevron(), CreateChevron());

        LineRenderer CreateChevron() {
            var chevron = new GameObject("TrackSegmentHelper_Chevron") {
                transform = { parent = transform },
                layer = Layers.Clickable
            };

            var boxCollider = chevron.AddComponent<BoxCollider>();
            boxCollider.size = new Vector3(0.4f, 0.4f, 0.8f);

            var lineRenderer = AddLineRenderer(chevron);
            lineRenderer.positionCount = 3;
            lineRenderer.SetPosition(0, new Vector3(-0.15f, 0, -0.3f));
            lineRenderer.SetPosition(1, new Vector3(0, 0, 0.45f));
            lineRenderer.SetPosition(2, new Vector3(0.15f, 0, -0.3f));
            return lineRenderer;
        }
    }

    private record Chevrons(LineRenderer Start, LineRenderer End);

    private TooltipInfo BuildTooltipInfo() {
        var sb = new StringBuilder();
        sb.AppendLine($"ID: {_TrackSegment.id}");
        sb.AppendLine($"Start: {_TrackSegment.a.id}");
        sb.AppendLine($"TailStyle: {_TrackSegment.b.id}");
        sb.AppendLine($"Priority: {_TrackSegment.priority}");
        sb.AppendLine($"Speed: {_TrackSegment.speedLimit}");
        sb.AppendLine($"GroupId: {_TrackSegment.groupId}");
        sb.AppendLine($"Style: {_TrackSegment.style}");
        sb.AppendLine($"Class: {_TrackSegment.trackClass}");
        sb.AppendLine($"Length: {_TrackSegment.Curve.CalculateLength()}m");
        return new TooltipInfo($"Segment {_TrackSegment.id}", sb.ToString());
    }

    #region IPickable

    public void Activate(PickableActivateEvent evt) {
        MapEditorPlugin.UpdateState(state => state with { SelectedAsset = _TrackSegment });
    }

    public void Deactivate() {
    }

    public float                    MaxPickDistance  => MapEditorPlugin.State.SelectedPatch != null ? 200f : 0f;
    public int                      Priority         => 1;
    public TooltipInfo              TooltipInfo      => MapEditorPlugin.State.SelectedPatch != null ? BuildTooltipInfo() : TooltipInfo.Empty;
    public PickableActivationFilter ActivationFilter => PickableActivationFilter.PrimaryOnly;

    #endregion
}
