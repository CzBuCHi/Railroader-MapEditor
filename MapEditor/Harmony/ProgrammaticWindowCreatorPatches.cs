using System;
using System.Linq;
using System.Reflection;
using HarmonyLib;
using JetBrains.Annotations;
using MapEditor.UI;
using UI;

namespace MapEditor.Harmony;

[PublicAPI]
[HarmonyPatch]
public static class ProgrammaticWindowCreatorPatches
{
    [HarmonyPostfix]
    [HarmonyPatch(typeof(ProgrammaticWindowCreator), "Start")]
    public static void Start(ProgrammaticWindowCreator __instance) {
        var methodInfo =
            typeof(ProgrammaticWindowCreator)
                .GetMethods(BindingFlags.Instance | BindingFlags.NonPublic)
                .FirstOrDefault(o => o.IsGenericMethod && o.Name == "CreateWindow" && o.GetParameters().Length == 1);

        if (methodInfo == null) {
            throw new InvalidOperationException("Cannot find method UI.ProgrammaticWindowCreator:CreateWindow<TWindow>(Action<>)");
        }
        
        methodInfo.MakeGenericMethod(typeof(EditorWindow)).Invoke(__instance, [new Action<EditorWindow>(_ => { })]);
        methodInfo.MakeGenericMethod(typeof(MilestonesWindow)).Invoke(__instance, [new Action<MilestonesWindow>(_ => { })]);
        methodInfo.MakeGenericMethod(typeof(SceneWindow)).Invoke(__instance, [new Action<SceneWindow>(_ => { })]);
    }
}
