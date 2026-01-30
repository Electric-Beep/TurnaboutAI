using HarmonyLib;
using System.Reflection;

namespace TurnaboutAI.Patches
{
    /// <summary>
    /// Handler for the point minigame. It's the parts during court where you must
    /// point out a spot in an image. There's only 1 correct answer.
    /// </summary>
    [HarmonyPatch]
    internal static class PointMiniGamePatch
    {
        private readonly static FieldInfo GetPoints;

        static PointMiniGamePatch()
        {
            GetPoints = typeof(PointMiniGame).GetField("converted_point_", BindingFlags.NonPublic | BindingFlags.Instance);

            if(GetPoints == null)
            {
                Plugin.LogInfo("PointMiniGame had a crazy problem!");
            }
        }

        [HarmonyPatch(typeof(PointMiniGame), nameof(PointMiniGame.Init))]
        [HarmonyPostfix]
        static void PointMiniGameInit()
        {
            var points = (GSPoint4[])GetPoints.GetValue(PointMiniGame.instance);
            ModStateManager.Instance.PointMiniGame(points[0]);
        }
    }
}
