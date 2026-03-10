using HarmonyLib;
using System.Collections;
using TurnaboutAI.Utility;

namespace TurnaboutAI.Patches
{
    /// <summary>
    /// Handles the "Press Enter" screen at game launch.
    /// </summary>
    [HarmonyPatch]
    internal static class StartPatch
    {
        [HarmonyPatch(typeof(startCtrl), nameof(startCtrl.Play))]
        [HarmonyPostfix]
        static void Play()
        {
            coroutineCtrl.instance.Play(new SequenceEnumerator(Skip()));
        }

        static IEnumerator Skip()
        {
            yield return Waiter.Wait(1);
            KeyPresser.PressKey(UnityEngine.KeyCode.Return);
        }
    }
}
