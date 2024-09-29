using System;
using System.Collections;
using JetBrains.Annotations;
using Serilog;
using UnityEngine;

namespace MapEditor.Utility;

public sealed class UnityHelpers : MonoBehaviour
{
    private static UnityHelpers? _Instance;

    [UsedImplicitly]
    private void Awake()
    {
        if (_Instance == null)
        {
            _Instance = this;
        }
        else
        {
            Destroy(gameObject);
        }
    }

    public static void StartStaticCoroutine(IEnumerator coroutine)
    {
        if (_Instance == null)
        {
            var go = new GameObject("CoroutineHelper");
            _Instance = go.AddComponent<UnityHelpers>();
        }

        _Instance.StartCoroutine(coroutine);
    }

    public static void CallOnNextFrame(Action action)
    {
        StartStaticCoroutine(CreateCoroutine());
        return;

        IEnumerator CreateCoroutine()
        {
            yield return new WaitForEndOfFrame();
            action();
        }
    }

    public static GameObject CreateGameObject(string name, Action<GameObject> initialize)
    {
        var go = new GameObject(name);
        go.SetActive(false);
        initialize(go);
        go.SetActive(true);
        return go;
    }
}
