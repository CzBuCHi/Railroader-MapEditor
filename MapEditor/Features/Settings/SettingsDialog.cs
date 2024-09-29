using System;
using System.Collections.Generic;
using System.Linq;
using MapEditor.Features.Abstract;
using Railloader;
using UI.Builder;
using UI.Common;
using UnityEngine;

namespace MapEditor.Features.Settings;

using static Window;

public sealed class SettingsDialog(IUIHelper uiHelper) : DialogBase(uiHelper)
{
    #region Manage

    private static SettingsDialog? _Instance;

    public static void Show(IUIHelper uiHelper) {
        Show(ref _Instance, () => new SettingsDialog(uiHelper));
    }

    public static void Close() {
        Close(ref _Instance);
    }

    #endregion

    private const string RedColor   = "FF7F7F";
    private const string WhiteColor = "FFFFFF";

    protected override int      WindowWidth    => 500;
    protected override int      WindowHeight   => 400;
    protected override Position WindowPosition => Position.Center;
    protected override string   WindowTitle    => "Map Editor | Settings";

    protected override void BuildWindow(UIPanelBuilder builder) {
        if (!_Initialized) {
            AssignToDictionary(_Initial);
            AssignToDictionary(_KeyCodes);
            _Initialized = true;
        }

        var conflict = _KeyCodes.GroupBy(o => o.Value, _ => 1).Any(o => o.Count() > 1);

        builder.AddSection("Keyboard", section => {
            BuildField(builder, section, "Move Forward | Roll Left", nameof(MapEditorSettings.KeyCodeForward));
            BuildField(builder, section, "Move Backward | Roll Right", nameof(MapEditorSettings.KeyCodeBackward));
            BuildField(builder, section, "Move Left | Yaw Left", nameof(MapEditorSettings.KeyCodeLeft));
            BuildField(builder, section, "Move Right | Yaw Right", nameof(MapEditorSettings.KeyCodeRight));
            BuildField(builder, section, "Move Up | Pitch Up", nameof(MapEditorSettings.KeyCodeUp));
            BuildField(builder, section, "Move Down | Pitch Down", nameof(MapEditorSettings.KeyCodeDown));

            section.AddLabel("Click on button and then press your preferred key ...");

            if (conflict) {
                section.AddLabel("Conflicting key bindings detected".Color(RedColor)!);
            }
        });

        builder.AddExpandingVerticalSpacer();
        builder.AddButton("Save changes", SaveSettings()).Disable(conflict);
    }

    private          bool                        _Initialized;
    private          string?                     _Listening;
    private readonly Dictionary<string, KeyCode> _Initial  = new();
    private readonly Dictionary<string, KeyCode> _KeyCodes = new();

    private void BuildField(UIPanelBuilder builder, UIPanelBuilder section, string text, string identifier) {
        var value = _Listening == identifier ? "Waiting ..." : _KeyCodes[identifier].ToString();

        section.AddField(GetLabel(),
            section.ButtonStrip(strip => {
                strip.AddButton(value, OnClick).RectTransform!.FlexibleWidth(100);
                if (_KeyCodes[identifier] != _Initial[identifier]) {
                    strip.AddButton("Reset", OnReset);
                }
            })!
        );

        return;

        void OnClick() {
            _Listening = identifier;
            builder.Rebuild();

            MapEditorPlugin.Listener.Listen(code => {
                _KeyCodes[identifier] = code;
                _Listening = null;
                builder.Rebuild();
            });
        }

        void OnReset() {
            _KeyCodes[identifier] = _Initial[identifier];
            builder.Rebuild();
        }

        string GetLabel() {
            var current = _KeyCodes[identifier];
            if (_KeyCodes.Any(o => o.Key != identifier && o.Value == current)) {
                return text.Color(RedColor)!;
            }

            return current == _Initial[identifier] ? text : text.Color(WhiteColor)!;
        }
    }

    private Action SaveSettings() => () => {
        MapEditorPlugin.UpdateSettings(o => o with {
            KeyCodeForward = _KeyCodes[nameof(MapEditorSettings.KeyCodeForward)],
            KeyCodeBackward = _KeyCodes[nameof(MapEditorSettings.KeyCodeBackward)],
            KeyCodeLeft = _KeyCodes[nameof(MapEditorSettings.KeyCodeLeft)],
            KeyCodeRight = _KeyCodes[nameof(MapEditorSettings.KeyCodeRight)],
            KeyCodeUp = _KeyCodes[nameof(MapEditorSettings.KeyCodeUp)],
            KeyCodeDown = _KeyCodes[nameof(MapEditorSettings.KeyCodeDown)]
        });

        Close();
    };

    private static void AssignToDictionary(Dictionary<string, KeyCode> dict) {
        dict[nameof(MapEditorSettings.KeyCodeForward)] = MapEditorPlugin.Settings.KeyCodeForward;
        dict[nameof(MapEditorSettings.KeyCodeBackward)] = MapEditorPlugin.Settings.KeyCodeBackward;
        dict[nameof(MapEditorSettings.KeyCodeLeft)] = MapEditorPlugin.Settings.KeyCodeLeft;
        dict[nameof(MapEditorSettings.KeyCodeRight)] = MapEditorPlugin.Settings.KeyCodeRight;
        dict[nameof(MapEditorSettings.KeyCodeUp)] = MapEditorPlugin.Settings.KeyCodeUp;
        dict[nameof(MapEditorSettings.KeyCodeDown)] = MapEditorPlugin.Settings.KeyCodeDown;
    }
}
