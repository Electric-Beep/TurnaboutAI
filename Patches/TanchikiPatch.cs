using TurnaboutAI.Actions;
using HarmonyLib;
using System.Collections.Generic;

namespace TurnaboutAI.Patches
{
    /// <summary>
    /// Handler for the Detect minigames. There's 1 section in games 2 and 3.
    /// </summary>
    [HarmonyPatch]
    internal static class TanchikiPatch
    {
        private static GSPoint4[] PointSet = null;
        private static bool[] Checked = null;

        public static void FlagChecked(int index)
        {
            if(Checked != null && Checked.Length > index)
            {
                Checked[index] = true;
            }
        }

        [HarmonyPatch(typeof(TanchikiMiniGame), nameof(TanchikiMiniGame.init))]
        [HarmonyPostfix]
        static void Init()
        {
            if(TanchikiMiniGame.find_target == null)
            {
                Plugin.LogError("No points? How'd you get here?");
                return;
            }

            if(PointSet != TanchikiMiniGame.find_target)
            {
                PointSet = TanchikiMiniGame.find_target;
                Checked = new bool[PointSet.Length];
            }

            List<DetectPoint> points = new List<DetectPoint>();
            for(int i = 0; i < PointSet.Length; i++)
            {
                if (Checked[i]) continue;

                points.Add(new DetectPoint
                {
                    Index = i,
                    Name = $"Spot {i}",
                    HitBox = PointSet[i],
                });
            }

            ModStateManager.Instance.Detect(points);
        }
    }
}
