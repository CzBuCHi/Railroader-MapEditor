using HarmonyLib;
using Helpers;
using JetBrains.Annotations;

namespace MapEditor.Features.SceneryAssetEditor.Harmony;

[PublicAPI]
[HarmonyPatch]
internal static class SceneryAssetInstancePatches
{
    [HarmonyPostfix]
    [HarmonyPatch(typeof(SceneryAssetInstance), nameof(OnEnable))]
    public static void OnEnable(SceneryAssetInstance __instance) {
    }
}
