using HarmonyLib;
using NeuroSdk.Actions;
using TMPro;
using UnityEngine;

namespace NeuroWordPlay.Tweaks;

[HarmonyPatch(typeof(TitleScreenManager))]
public class TitleScreen
{
    [HarmonyPostfix]
    [HarmonyPatch("Start")]
    static void Start(TitleScreenManager __instance, TMP_Text ___versionNo)
    {
        ___versionNo.enableWordWrapping = false;
        Plugin.Logger.LogDebug("Last tested on 1.09");
        if (Application.version.StartsWith("1.09"))
        {
            ___versionNo.text += " (Neuro SDK)";
        }
        else
        {
            ___versionNo.text += " (UNTESTED Neuro SDK)";
        }

        NeuroActionHandler.UnregisterActions(["saveandquit"]);

        if (MetaGameRules.Instance.hasInProgressData)
        {
            ActionWindow.Create(__instance.levelSelectGO)
                .SetForce(10, "", "", false)
                .AddAction(new ResumeGameAction(__instance))
                .AddAction(new GoToLevelSelectAction(__instance))
                .Register();
            return;
        }

        ActionWindow.Create(__instance.levelSelectGO)
            .SetForce(10, "", "", false)
            .AddAction(new GoToLevelSelectAction(__instance))
            .Register();
    }

    [HarmonyPostfix]
    [HarmonyPatch(nameof(TitleScreenManager.OpenPage))]
    static void OpenPage(TitleScreenManager __instance, GameObject exitingPage, GameObject enteringPage,
        TitleControl.TitleControlZone enteringZone)
    {
        if (enteringZone != TitleControl.TitleControlZone.LevelSelect) return;

        ActionWindow.Create(__instance.levelSelectGO)
            .SetForce(10, "", "", false)
            .AddAction(new LevelSelectionAction(__instance.levelSelectGO))
            .Register();
    }
}
