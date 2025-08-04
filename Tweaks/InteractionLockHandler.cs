using HarmonyLib;

[HarmonyPatch(typeof(ControlManagerScript))]
public class InteractionLockHandler
{
    [HarmonyPostfix]
    [HarmonyPatch(nameof(ControlManagerScript.PausePlayerInteraction))]
    static void PausePlayerInteraction(ControlManagerScript __instance)
    {
        InteractionLockHelper.IsLocked = true;
    }

    [HarmonyPostfix]
    [HarmonyPatch(nameof(ControlManagerScript.ResumePlayerInteraction))]
    static void ResumePlayerInteraction(ControlManagerScript __instance)
    {
        InteractionLockHelper.IsLocked = false;
        InteractionLockHelper.ProcessQueue();
    }
}