using System;
using System.Reflection;
using BepInEx;
using BepInEx.Logging;
using HarmonyLib;
using NeuroSdk;
using UnityEngine;

namespace NeuroWordPlay;

[BepInPlugin(MyPluginInfo.PLUGIN_GUID, MyPluginInfo.PLUGIN_NAME, MyPluginInfo.PLUGIN_VERSION)]
public class Plugin : BaseUnityPlugin
{
    internal new static ManualLogSource Logger;

    private void Awake()
    {
        // Plugin startup logic
        Logger = base.Logger;

        Environment.SetEnvironmentVariable("NEURO_SDK_WS_URL", Environment.GetEnvironmentVariable("NEURO_SDK_WS_URL") ?? "ws://localhost:8000");

        NeuroSdkSetup.Initialize("Word Play");

        Harmony.CreateAndPatchAll(Assembly.GetExecutingAssembly());

        Logger.LogInfo($"Plugin {MyPluginInfo.PLUGIN_GUID} is loaded!");

        PlayerPrefs.SetInt("tutorials", 0);

        Logger.LogDebug("Disabled tutorials");
    }
}
