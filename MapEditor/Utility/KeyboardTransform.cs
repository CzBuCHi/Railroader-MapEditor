using System;
using System.Collections.Generic;
using JetBrains.Annotations;
using MapEditor.Features.Abstract;
using MapEditor.Features.SceneryAssetEditor;
using MapEditor.Features.TelegraphPoleEditor;
using MapEditor.Features.TrackNodeEditor;
using UnityEngine;

namespace MapEditor.Utility;

[PublicAPI]
public class KeyboardTransform : MonoBehaviour
{
    private static KeyboardTransform? _Instance;

    public static void Initialize() {
        var gameObject = new GameObject("KeyboardTransform");
        _Instance = gameObject.AddComponent<KeyboardTransform>();
    }

    public static void Destroy() {
        Destroy(_Instance!.gameObject);
        _Instance = null;
    }

    private IKeyboardTransformer? _Transformer;

    private Dictionary<KeyCode, float> _Times = new();

    private IKeyboardTransformer? GetTransformer(MapEditorState state) {
        return state switch {
            { TrackNode: not null }            => GetOrCreateTransformer<TrackNodeTransformer>(),
            { TelegraphPole: not null }        => GetOrCreateTransformer<TelegraphPoleTransformer>(),
            { SceneryAssetInstance: not null } => GetOrCreateTransformer<SceneryAssetTransformer>(),
            _                                  => null
        };

        IKeyboardTransformer GetOrCreateTransformer<TTransformer>() where TTransformer : class, IKeyboardTransformer, new() => _Transformer as TTransformer ?? new TTransformer();
    }

    public void Update() {
        _Transformer = GetTransformer(MapEditorPlugin.State);
        if (_Transformer == null) {
            return;
        }

        TrackKey(_Transformer, MapEditorPlugin.Settings.KeyCodeForward, KeyboardTransformDirection.Forward);
        TrackKey(_Transformer, MapEditorPlugin.Settings.KeyCodeBackward, KeyboardTransformDirection.Backward);
        TrackKey(_Transformer, MapEditorPlugin.Settings.KeyCodeLeft, KeyboardTransformDirection.Left);
        TrackKey(_Transformer, MapEditorPlugin.Settings.KeyCodeRight, KeyboardTransformDirection.Right);
        TrackKey(_Transformer, MapEditorPlugin.Settings.KeyCodeUp, KeyboardTransformDirection.Up);
        TrackKey(_Transformer, MapEditorPlugin.Settings.KeyCodeDown, KeyboardTransformDirection.Down);
    }

    private void TrackKey(IKeyboardTransformer transformer, KeyCode key, KeyboardTransformDirection direction) {
        if (Input.GetKeyDown(key)) {
            _Times[key] = Time.time;
            transformer.TransformBegin();
        }

        if (Input.GetKeyUp(key)) {
            _Times.Remove(key);
            transformer.TransformComplete();
        }

        if (_Times.TryGetValue(key, out var time)) {
            transformer.Transform(Time.time - time, direction);
        }
    }

    internal static void Move(Transform transform, float delta, KeyboardTransformDirection direction) {
        Move(transform, Vector3.one * delta, direction);
    }

    internal static void Move(Transform transform, Vector3 delta, KeyboardTransformDirection direction) {
        transform.localPosition += direction switch {
            KeyboardTransformDirection.Up       => transform.up * delta.x,
            KeyboardTransformDirection.Down     => transform.up * -delta.x,
            KeyboardTransformDirection.Left     => transform.right * -delta.y,
            KeyboardTransformDirection.Right    => transform.right * delta.y,
            KeyboardTransformDirection.Forward  => transform.forward * delta.z,
            KeyboardTransformDirection.Backward => transform.forward * -delta.z,
            _                                   => throw new ArgumentOutOfRangeException(nameof(direction), direction, null!)
        };
    }

    internal static void Rotate(Transform transform, float delta, KeyboardTransformDirection direction) {
        Rotate(transform, Vector3.one * delta, direction);
    }

    internal static void Rotate(Transform transform, Vector3 delta, KeyboardTransformDirection direction) {
        transform.localEulerAngles += direction switch {
            KeyboardTransformDirection.Up       => Vector3.forward * delta.x,
            KeyboardTransformDirection.Down     => Vector3.back * delta.x,
            KeyboardTransformDirection.Left     => Vector3.down * delta.y,
            KeyboardTransformDirection.Right    => Vector3.up * delta.y,
            KeyboardTransformDirection.Forward  => Vector3.right * delta.z,
            KeyboardTransformDirection.Backward => Vector3.left * delta.z,
            _                                   => throw new ArgumentOutOfRangeException(nameof(direction), direction, null!)
        };
    }
}

public enum KeyboardTransformDirection
{
    Forward,
    Backward,
    Left,
    Right,
    Up,
    Down
}
