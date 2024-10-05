using System;
using System.Collections;
using System.Collections.Concurrent;
using System.Collections.Generic;
using Cameras;
using JetBrains.Annotations;
using MapEditor.Utility.Harmony;
using UnityEngine;

namespace MapEditor.Utility;

[UsedImplicitly]
public sealed class UnityHelpers : MonoBehaviour
{
    public static void Initialize() {
        var go = new GameObject("UnityHelpers");
        _Instance = go.AddComponent<UnityHelpers>();
    }

    private static UnityHelpers? _Instance;

    private static void StartStaticCoroutine(IEnumerator coroutine) {
        if (_Instance == null) {
            var go = new GameObject("CoroutineHelper");
            _Instance = go.AddComponent<UnityHelpers>();
        }

        _Instance.StartCoroutine(coroutine);
    }

    public static void CallOnNextFrame(Action action) {
        StartStaticCoroutine(CreateCoroutine());
        return;

        IEnumerator CreateCoroutine() {
            yield return new WaitForEndOfFrame();
            action();
        }
    }

    public static GameObject CreateGameObject(string name, Action<GameObject> initialize) {
        var go = new GameObject(name);
        go.SetActive(false);
        initialize(go);
        go.SetActive(true);
        return go;
    }

    private static readonly ConcurrentDictionary<int, List<Action>> _CallOnceOnMouseButtonHandlers = new();
    
    public void Update() {
        var handlers = _CallOnceOnMouseButtonHandlers;
        if (handlers.Count == 0) {
            return;
        }

        foreach (var pair in handlers) {
            if (!Input.GetMouseButton(pair.Key)) {
                continue;
            }

            var actions = pair.Value!.ToArray();
            pair.Value.Clear();
            foreach (var action in actions) {
                action();
            }
        }
        
    }

    public static void CallOnceOnMouseButton(int button, Action action) {
        var list = _CallOnceOnMouseButtonHandlers.GetOrAdd(button, new List<Action>())!;
        list.Add(action);
    }

    public static Vector3 RayPointFromMouse() {
        CameraSelector.shared.strategyCamera.RayPointFromMouse(out var point);
        return point;
    }
}
