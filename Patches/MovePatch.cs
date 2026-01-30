using HarmonyLib;
using System.Collections.Generic;

namespace TurnaboutAI.Patches
{
    /// <summary>
    /// Handler for the move control to transition between locations.
    /// </summary>
    [HarmonyPatch]
    internal static class MovePatch
    {
        [HarmonyPatch(typeof(moveCtrl), nameof(moveCtrl.play))]
        [HarmonyPostfix]
        static void MovePlay()
        {
            List<string> locations = new List<string>();

            foreach (var plate in moveCtrl.instance.select_list)
            {
                if (!plate.active) continue;

                Plugin.LogInfo(plate.text);
                locations.Add(plate.text);
            }

            ModStateManager.Instance.Move(locations);
        }
    }
}
