using System;
using Cameras;
using HarmonyLib;
using JetBrains.Annotations;
using UnityEngine;

namespace MapEditor.Utility.Harmony;

[PublicAPI]
[HarmonyPatch]
public static class StrategyCameraControllerPatches
{
    [HarmonyReversePatch]
    [HarmonyPatch(typeof(StrategyCameraController), nameof(RayPointFromMouse))]
    public static bool RayPointFromMouse(this StrategyCameraController __instance, out Vector3 point) {
        throw new Exception("HarmonyReversePatch");
    }
}
