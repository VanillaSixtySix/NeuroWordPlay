using System.Collections.Generic;
using HarmonyLib;

namespace NeuroWordPlay.Behaviors;

[HarmonyPatch(typeof(KeyboardControl))]
public class Keyboard
{
    [HarmonyPrefix]
    [HarmonyPatch("Update")]
    static void Update(KeyboardControl __instance, Queue<string> ___keyBuffer)
    {
        if (KeyboardQueue.Queue.Count > 0)
        {
            var text = KeyboardQueue.Queue.Dequeue();
            ___keyBuffer.Enqueue(text);
		    ControlManagerScript.Instance.CheckIfNeedToEndDrag(false);
            ControlManagerScript.Instance.hasTyped = true;
        }
    }
}
