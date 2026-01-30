using HarmonyLib;
using System.Collections.Generic;
using TurnaboutAI.Actions;
using TurnaboutAI.Utility;

namespace TurnaboutAI.Patches
{
    /// <summary>
    /// Used for Examine sections.
    /// </summary>
    [HarmonyPatch]
    internal static class InspectPatch
    {
        [HarmonyPatch(typeof(inspectCtrl), nameof(inspectCtrl.play))]
        [HarmonyPrefix]
        static void InspectPlay()
        {
            var data = GSStatic.inspect_data_;

            if (data != null)
            {
                List<ExamineSpot> spots = new List<ExamineSpot>();

                for(int i = 0; i < data.Length; i++)
                {
                    INSPECT_DATA inspect = data[i];

                    if (inspect.message == uint.MaxValue) continue;

                    uint nextMsg = inspectCtrl.GetNextInspectNumber(inspect.message);
                    if (GSStatic.global_work_.inspect_readed_[0, nextMsg] == 1) continue;

                    spots.Add(new ExamineSpot
                    {
                        Name = $"Spot {i + 1}",
                        Polygon = new Polygon(inspect)
                    });
                }

                ModStateManager.Instance.Inspect(spots, true);
            }
        }
    }
}
