using HarmonyLib;

namespace TurnaboutAI.Patches
{
    /// <summary>
    /// Handler for the main menu.
    /// </summary>
    [HarmonyPatch]
    internal static class MainTitlePatch
    {
        [HarmonyPatch(typeof(mainTitleCtrl), nameof(mainTitleCtrl.Init))]
        [HarmonyPrefix]
        static void Init()
        {
            ModStateManager.Instance.UnregisterAllActions();
        }
    }
}
