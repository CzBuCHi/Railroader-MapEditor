using System.IO;
using System.Text;
using Helpers;
using Track;

namespace MapEditor;

public record MapEditorState
{
    public string?       SelectedPatch { get; init; }
    public TransformMode TransformMode { get; init; } = TransformMode.Move;
    public bool          ShowSpans     { get; init; }
    public object?       SelectedAsset { get; init; }

    public TrackNode?            TrackNode            => SelectedAsset as TrackNode;
    public TrackSegment?         TrackSegment         => SelectedAsset as TrackSegment;
    public TelegraphPoleId?      TelegraphPole        => SelectedAsset as TelegraphPoleId;
    public SceneryAssetInstance? SceneryAssetInstance => SelectedAsset as SceneryAssetInstance;

    public override string ToString() {
        StringBuilder sb = new StringBuilder();
        sb.Append("MapEditorState {");
        sb.Append("SelectedPatch = ");
        if (SelectedPatch != null) {
            sb.Append(Path.GetFileName(Path.GetDirectoryName(SelectedPatch)!)).Append("\\").Append(Path.GetFileName(SelectedPatch));
        }

        sb.Append(", TransformMode = ").Append(TransformMode);
        sb.Append(", SelectedAsset = ");
        if (SelectedAsset != null) {
            sb.Append(SelectedAsset);
        }

        sb.Append(" }");
        return sb.ToString();
    }
}

public record TelegraphPoleId(int Id);

public enum TransformMode
{
    Move,
    Rotate
}
