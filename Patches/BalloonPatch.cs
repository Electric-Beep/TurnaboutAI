using HarmonyLib;

namespace TurnaboutAI.Patches
{
    /// <summary>
    /// Interjections such as Objection!
    /// </summary>
    [HarmonyPatch]
    internal static class BalloonPatch
    {
        [HarmonyPatch(typeof(Balloon), nameof(Balloon.PlayTakeThat))]
        [HarmonyPostfix]
        static void TakeThat()
        {
            ModStateManager.Instance.SendContext("Take That!", false);
        }

        [HarmonyPatch(typeof(Balloon), nameof(Balloon.PlayHoldIt))]
        [HarmonyPostfix]
        static void HoldIt()
        {
            ModStateManager.Instance.SendContext("Hold It!", false);
        }

        [HarmonyPatch(typeof(Balloon), nameof(Balloon.PlayObjection))]
        [HarmonyPostfix]
        static void Objection()
        {
            ModStateManager.Instance.SendContext("Objection!", false);
        }
    }
}
