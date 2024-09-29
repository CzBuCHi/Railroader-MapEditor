using MapEditor.Features.Settings;
using UnityEngine;

namespace MapEditor;

public sealed partial class MapEditorPlugin
{
    private static KeyPressListener? _Listener;

    public static KeyPressListener Listener => _Listener ??= CreateListener();

    private static KeyPressListener CreateListener() {
        var go = new GameObject("KeyPressListener");
        go.SetActive(false);
        var listener = go.AddComponent<KeyPressListener>();
        listener.enabled = false;
        go.SetActive(true);
        return listener;
    }
}
