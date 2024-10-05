using System;
using System.Collections.Generic;
using System.Linq;
using Helpers;
using MapEditor.Utility;
using Newtonsoft.Json.Linq;
using Serilog;
using StrangeCustoms.Tracks;
using UnityEngine;
using Object = UnityEngine.Object;
using  MapEditor.Extensions;

namespace MapEditor.Features.SceneryAssetEditor;

public static class SceneryAssetUtility
{
    public static readonly List<string> Identifiers = Object.FindObjectsOfType<SceneryAssetInstance>()!.Select(o => o.identifier).Distinct().ToList();

    public static void CreateNew() {
        if (!WorldTransformer.TryGetShared(out var worldTransformer) || worldTransformer == null) {
            throw new InvalidOperationException("Cannot get world transformer");
        }

        var scenery = worldTransformer.transform.GetChild("Large Scenery");
        if (scenery == null) {
            throw new InvalidOperationException("Cannot get large scenery");
        }

        var wrapper = new GameObject("Editor");
        wrapper.transform.SetParent(scenery);

        UnityHelpers.CallOnceOnMouseButton(0, () => {
            var point = UnityHelpers.RayPointFromMouse().WorldToGame();

            var serializedScenery = new SerializedScenery {
                ModelIdentifier = "freight-house-general",
                Position = point,
                Rotation = Vector3.zero,
                Scale = Vector3.one,
                ExtraData = new Dictionary<string, JToken> {
                    { "ID", JToken.FromObject("IDD") }
                }
            };

            var gameObject = new GameObject(serializedScenery.ExtraData["ID"]!.Value<string>()!);
            gameObject.SetActive(false);
            gameObject.transform.SetParent(wrapper.transform);
            var sceneryAssetInstance = gameObject.AddComponent<SceneryAssetInstance>();
            sceneryAssetInstance.identifier = serializedScenery.ModelIdentifier;
            sceneryAssetInstance.transform.SetPositionAndRotation(serializedScenery.Position, Quaternion.Euler(serializedScenery.Rotation));
            sceneryAssetInstance.transform.localScale = serializedScenery.Scale;
            worldTransformer.AddObjectToMove(sceneryAssetInstance.transform);
            sceneryAssetInstance.gameObject.SetActive(true);

            MapEditorPlugin.UpdateState(state => state with { SelectedAsset = sceneryAssetInstance });
        });
    }

    public static void Show() {
        CameraSelector.shared.ZoomToPoint(MapEditorPlugin.State.SceneryAssetInstance!.transform.position);
    }

    public static void Remove() {
        Object.Destroy(MapEditorPlugin.State.SceneryAssetInstance!.gameObject);
        MapEditorPlugin.UpdateState(state => state with { SelectedAsset = null });
    }
}

