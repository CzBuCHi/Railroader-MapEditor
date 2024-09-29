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
}

public record TelegraphPoleId(int Id);

public enum TransformMode
{
    Move,
    Rotate
}
