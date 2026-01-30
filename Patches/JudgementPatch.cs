using HarmonyLib;

namespace TurnaboutAI.Patches
{
    /// <summary>
    /// Handler for the Guilty/Not Guilty verdicts.
    /// </summary>
    [HarmonyPatch]
    internal static class JudgementPatch
    {
        [HarmonyPatch(typeof(judgmentCtrl), nameof(judgmentCtrl.judgment))]
        [HarmonyPrefix]
        static void Judgement(int in_type)
        {
            if(in_type == 0)
            {
                ModStateManager.Instance.SendContext("NOT GUILTY", false);
            }
            else
            {
                ModStateManager.Instance.SendContext("GUILTY", false);
            }
        }
    }
}
