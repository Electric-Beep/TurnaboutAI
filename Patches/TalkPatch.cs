using HarmonyLib;
using System.Collections.Generic;
using System.Linq;

namespace TurnaboutAI.Patches
{
    /// <summary>
    /// Handler for the selection control, used during Talk and Select windows.
    /// </summary>
    [HarmonyPatch]
    internal static class TalkPatch
    {
        private readonly static List<TalkOption> selectOptions = new List<TalkOption>();

        /// <summary>
        /// This is called when all dialog options have been loaded.
        /// </summary>
        [HarmonyPatch(typeof(selectPlateCtrl), nameof(selectPlateCtrl.stopCursor))]
        [HarmonyPrefix]
        static void StopCursor()
        {
            if (selectOptions.Count == 0) return;

            //ToList to create a copy of the list.
            ModStateManager.Instance.SetTalkOptions(selectOptions.ToList(), selectPlateCtrl.instance.is_select);
        }

        /// <summary>
        /// Called when an option is added.
        /// </summary>
        [HarmonyPatch(typeof(selectPlateCtrl), nameof(selectPlateCtrl.setText))]
        [HarmonyPrefix]
        static void SetText(int index, string text)
        {
            if (index == 0)
            {
                selectOptions.Clear();
            }

            selectOptions.Add(new TalkOption
            {
                Text = text,
            });
        }

        /// <summary>
        /// Called to mark an option as read or not.
        /// </summary>
        [HarmonyPatch(typeof(selectPlateCtrl), nameof(selectPlateCtrl.setRead))]
        [HarmonyPrefix]
        static void SetRead(int index, bool is_read, bool is_psylooc = false)
        {
            if (index < selectOptions.Count)
            {
                selectOptions[index].Read = is_read;
                selectOptions[index].PsyLock = is_psylooc;
            }
        }
    }

    public sealed class TalkOption
    {
        public bool PsyLock { get; set; }
        public bool Read { get; set; }
        public string Text { get; set; }

        public override string ToString()
        {
            return $"{{Read:{Read}, Text:{Text}}}";
        }
    }
}
