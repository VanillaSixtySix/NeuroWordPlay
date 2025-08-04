using HarmonyLib;

namespace NeuroWordPlay.Tweaks;

[HarmonyPatch(typeof(ReportScript))]
public class DisableReports
{
    [HarmonyPrefix]
    [HarmonyPatch(nameof(ReportScript.ChangeToReportWord))]
    [HarmonyPatch(nameof(ReportScript.ChangeBack))]
    [HarmonyPatch(nameof(ReportScript.ReportAnIssue))]
    static bool Prefix()
    {
        return false;
    }
}
