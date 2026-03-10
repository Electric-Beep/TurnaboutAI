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

            if (GSStatic.save_data[0].in_data == 0)
            {
                //No save in slot 0, assume new game.
                ModStateManager.Instance.MainMenuNewGame();
            }
            else
            {
                //Save in slot 0, assume continue.
                ModStateManager.Instance.MainMenuContinueGame();
            }
        }

        public static titleSelectPlate GetSelectPlate()
        {
            var field = typeof(mainTitleCtrl).GetField("select_plate_", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            return field.GetValue(mainTitleCtrl.instance) as titleSelectPlate;
        }
    }
}
