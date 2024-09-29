using System;
using System.Collections.Generic;
using System.Linq;
using Helpers;
using MapEditor.Features.SceneryAssetEditor.Visualizers;
using Newtonsoft.Json.Linq;
using StrangeCustoms.Tracks;
using UnityEngine;
using Object = UnityEngine.Object;

namespace MapEditor.Features.SceneryAssetEditor;

public static class SceneryAssetUtility
{
    public static readonly List<string> Identifiers = Object.FindObjectsOfType<SceneryAssetInstance>()!.Select(o => o.identifier).Distinct().ToList();


    public static void CreateNew() {
        if (!WorldTransformer.TryGetShared(out var worldTransformer) || worldTransformer == null) {
            throw new InvalidOperationException("Cannot get world transformer");
        }

        // random data - location in east whittier
        var serializedScenery = new SerializedScenery {
            ModelIdentifier = "freight-house-general",
            Position = new Vector3(12815.19f, 566.62f, 4667.18f),
            Rotation = new Vector3(0, 65.5f, 0),
            Scale = new Vector3(1.2f, 1.2f, 1.2f),
            ExtraData = new Dictionary<string, JToken> {
                { "ID", JToken.FromObject("IDD") }
            }
        };

        var gameObject = new GameObject(serializedScenery.ExtraData["ID"]!.Value<string>()!);
        gameObject.SetActive(false);
        var sceneryAssetInstance = gameObject.AddComponent<SceneryAssetInstance>();
        sceneryAssetInstance.identifier = serializedScenery.ModelIdentifier;
        sceneryAssetInstance.transform.SetPositionAndRotation(serializedScenery.Position, Quaternion.Euler(serializedScenery.Rotation));
        sceneryAssetInstance.transform.localScale = serializedScenery.Scale;
        worldTransformer.AddObjectToMove(sceneryAssetInstance.transform);
        sceneryAssetInstance.gameObject.SetActive(true);

        SceneryAssetVisualizerManager.CreateVisualizer(sceneryAssetInstance); // create yellow cube

        
    }
}
