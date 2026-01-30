using HarmonyLib;

namespace TurnaboutAI.Patches
{
    /// <summary>
    /// Handler for loading a save.
    /// </summary>
    [HarmonyPatch]
    internal static class LoadPatch
    {
        [HarmonyPatch(typeof(SaveControl), nameof(SaveControl.LoadGameData))]
        [HarmonyPrefix]
        static void LoadGameData(int slot)
        {
            Plugin.LogInfo($"Load Game Data - {slot}");

            ModStateManager.Instance.Load();
        }
    }
}
