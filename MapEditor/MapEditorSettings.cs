using UnityEngine;

namespace MapEditor;

public record MapEditorSettings
{
    // move / rotate buttons
    public KeyCode KeyCodeForward  { get; init; } = KeyCode.Keypad8;
    public KeyCode KeyCodeBackward { get; init; } = KeyCode.Keypad5;
    public KeyCode KeyCodeLeft     { get; init; } = KeyCode.Keypad4;
    public KeyCode KeyCodeRight    { get; init; } = KeyCode.Keypad6;
    public KeyCode KeyCodeUp       { get; init; } = KeyCode.Keypad9;
    public KeyCode KeyCodeDown     { get; init; } = KeyCode.Keypad3;
}
