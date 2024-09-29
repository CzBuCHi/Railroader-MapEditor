using System;
using System.IO;
using System.Reflection;
using Serilog;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;
using Object = UnityEngine.Object;

namespace MapEditor.TopRightArea;

public static class TopRightAreaExtension
{
    public static void AddButton(UnityAction onClick) {
        var topRightArea = Object.FindObjectOfType<global::UI.TopRightArea>();
        if (topRightArea == null) {
            return;
        }

        var strip = topRightArea.transform.Find("Strip");
        if (strip == null) {
            return;
        }

        var gameObject = new GameObject("MapEditorButton") {
            transform = { parent = strip }
        };
        gameObject.transform.SetSiblingIndex(9);

        var button = gameObject.AddComponent<Button>();
        button.onClick.AddListener(onClick);

        var image = gameObject.AddComponent<Image>();
        image.sprite = Sprite.Create(_ConstructionIcon, new Rect(0, 0, 24, 24), new Vector2(0.5f, 0.5f))!;
        image.rectTransform!.SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal, 32);
        image.rectTransform.SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical, 32);
    }

    private static readonly Texture2D _ConstructionIcon = LoadTexture2D("construction-icon.png", 24, 24);

    private static byte[] GetBytes(string path) {
        var       assembly = Assembly.GetExecutingAssembly();
        using var stream   = assembly.GetManifestResourceStream(path)!;
        using var ms       = new MemoryStream();
        stream.CopyTo(ms);
        return ms.ToArray();
    }

    private static Texture2D LoadTexture2D(string path, int width, int height) {
        try {
            var bytes   = GetBytes($"MapEditor.TopRightArea.{path}");
            var texture = new Texture2D(width, height);
            texture.LoadImage(bytes);
            return texture;
        } catch (Exception e) {
            Log.Error(e, "Failed to load texture {0}", path);
        }

        return null!;
    }
}
