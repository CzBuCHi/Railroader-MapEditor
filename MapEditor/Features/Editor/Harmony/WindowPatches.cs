using HarmonyLib;
using JetBrains.Annotations;
using UI.Common;

namespace MapEditor.Features.Editor.Harmony;

[PublicAPI]
[HarmonyPatch]
public static class WindowPatches
{
    [HarmonyReversePatch]
    [HarmonyPatch(typeof(Window), nameof(ClampToParentBounds))]
    public static void ClampToParentBounds(this Window __instance)
    {
    }
}