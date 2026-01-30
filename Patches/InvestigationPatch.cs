using HarmonyLib;

namespace TurnaboutAI.Patches
{
    /// <summary>
    /// Called Tantei in the game, it is the selection menu when interacting during the investigation sections.
    /// i.e. the Inspect, Move, Talk, Present buttons.
    /// </summary>
    [HarmonyPatch]
    internal static class InvestigationPatch
    {
        /// <summary>
        /// Sets what options the menu has.
        /// 0 = Inspect, Move
        /// 1 = Inspect, Move, Talk, Present
        /// </summary>
        /// <param name="in_type"></param>
        [HarmonyPatch(typeof(tanteiMenu), nameof(tanteiMenu.setMenu))]
        [HarmonyPrefix]
        static void SetMenu(int in_type)
        {
            ModStateManager.Instance.SetInvestigationMenu(in_type);
        }
    }
}
