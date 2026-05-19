using BepInEx;
using BepInEx.Logging;
using HarmonyLib;

namespace GWYFSlotCustomizer;

[BepInPlugin(PluginGuid, PluginName, PluginVersion)]
public class Plugin : BaseUnityPlugin
{
    public const string PluginGuid = "com.agonyz.gwyf.slotcustomizer";
    public const string PluginName = "GWYF Slot Customizer";
    public const string PluginVersion = "0.1.2";

    internal static ManualLogSource Log = null!;

    private readonly Harmony _harmony = new(PluginGuid);

    private void Awake()
    {
        Log = Logger;
        _harmony.PatchAll();
        Log.LogInfo($"{PluginName} v{PluginVersion} loaded.");
    }

    private void OnDestroy()
    {
        _harmony.UnpatchSelf();
    }
}