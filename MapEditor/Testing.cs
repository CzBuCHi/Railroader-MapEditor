#if DEBUG

using System.Collections.Generic;
using System.IO;
using System.Linq;
using Helpers;
using MapEditor.Features.SceneryAssetEditor;
using Newtonsoft.Json;
using SimpleGraph.Runtime;
using Track;
using UI.Builder;
using UnityEngine;

// ReSharper disable All

namespace MapEditor;

public static class Testing
{
    public static void Execute() {


        var sai = UnityEngine.Object.FindObjectOfType<SceneryAssetInstance>();

        var root = GetRoot(sai.transform);
        var go   = root.gameObject;
        var json = new JsonGameObject(go);

        


        //SceneryAssetUtility.CreateNew();
        

        File.WriteAllText(@"c:\projects\Railroader\Graph.json", JsonConvert.SerializeObject(json, Formatting.Indented));

        Transform GetRoot(Transform transform) {
            while(transform.parent != null) {
                transform = transform.parent;
            }

            return transform;
        }
    }
}

public class JsonGameObject(GameObject gameObject)
{
    public string           name        = gameObject.name;
    public string           position    = gameObject.transform.localPosition.ToString();

    public bool ShouldSerializeposition() {
        return gameObject.transform.localPosition != Vector3.zero;
    }

    public string           eulerAngles = gameObject.transform.localEulerAngles.ToString();

    public bool ShouldSerializeeulerAngles() {
        return gameObject.transform.localEulerAngles != Vector3.zero;
    }

    public string[]         components  = gameObject.GetComponents<Component>().Where(o => o is not Transform).Select(o => o.GetType().Name).ToArray();

    public bool ShouldSerializecomponents() {
        return components.Length > 0;
    }

    public JsonGameObject[] children = gameObject.EnumerateChildren().Where(o => o.name != "Track").Select(o => new JsonGameObject(o)).ToArray();

    public bool ShouldSerializechildren() {
        return children.Length > 0;
    }
}

public static class GameObjectEx
{
    public static IEnumerable<GameObject> EnumerateChildren(this GameObject gameObject) {
        for (int i = 0; i < gameObject.transform.childCount; i++) {
            yield return gameObject.transform.GetChild(i).gameObject;
        }
    }
}



public class JsonNode(Node node)
{
    public int    id          = node.id;
    public string position    = node.position.ToString();
    public string eulerAngles = node.eulerAngles.ToString();
    public string scale       = node.scale.ToString();
    public int    tag         = node.tag;
}


public class JsonTrackNode(TrackNode trackNode)
{
    public string Position        = trackNode.transform.position.ToString();
    public string Rotation        = trackNode.transform.eulerAngles.ToString();
    public bool   FlipSwitchStand = trackNode.flipSwitchStand;
}

public class JsonTrackSegment(TrackSegment trackSegment)
{
    public string             Position   = trackSegment.transform.position.ToString();
    public string             Rotation   = trackSegment.transform.eulerAngles.ToString();
    public TrackSegment.Style Style      = trackSegment.style;
    public TrackClass         TrackClass = trackSegment.trackClass;
    public string             StartId    = trackSegment.a.id;
    public string             EndId      = trackSegment.b.id;
    public int                Priority   = trackSegment.priority;
    public int                SpeedLimit = trackSegment.speedLimit;
    public string?            GroupId    = trackSegment.groupId;
}

#endif
