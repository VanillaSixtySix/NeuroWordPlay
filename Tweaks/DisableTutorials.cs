using HarmonyLib;

namespace NeuroWordPlay.Tweaks;

[HarmonyPatch(typeof(TutorialManager))]
public class DisableTutorials
{
    [HarmonyPrefix]
    [HarmonyPatch(nameof(TutorialManager.ShowInvalidTutorial))]
    [HarmonyPatch(nameof(TutorialManager.ShowTutorialScreen))]
    [HarmonyPatch(nameof(TutorialManager.ShowSpecialTypingTutorial))]
    [HarmonyPatch(nameof(TutorialManager.ShowPerkScreenTutorial))]
    [HarmonyPatch(nameof(TutorialManager.ShowUpgradeTutorial))]
    [HarmonyPatch(nameof(TutorialManager.ShowModifierTutorial))]
    static bool Prefix(ref bool __result)
    {
        __result = false;
        return false;
    }
}