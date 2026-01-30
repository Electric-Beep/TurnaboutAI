using HarmonyLib;
using System.Collections.Generic;
using System.Linq;

namespace TurnaboutAI.Patches
{
    /// <summary>
    /// Handler for the record control. This houses evidence and psychological profiles.
    /// </summary>
    [HarmonyPatch]
    internal static class RecordPatch
    {
        private static readonly List<Record> Records = new List<Record>();

        /// <summary>
        /// Gets all evidence.
        /// </summary>
        public static IEnumerable<Record> GetEvidence() => Records.Where(r => r.Type == 0);

        /// <summary>
        /// Gets all profiles.
        /// </summary>
        public static IEnumerable<Record> GetProfiles() => Records.Where(r => r.Type == 1);

        /// <summary>
        /// Called when the court record is opened.
        /// </summary>
        [HarmonyPatch(typeof(recordListCtrl), nameof(recordListCtrl.noteOpen))]
        [HarmonyPrefix]
        static void RecordNoteOpen(bool seal_key = false, int mode = 0, bool isFingerPrint = false)
        {
            //The is_back property determines if the court record can be closed.
            //The is_change property determines if the Profiles button is available.
            //Current record selection is probably record_data_[record_type].cursor_no_

            bool canGoBack = recordListCtrl.instance.is_back;
            bool canPresentProfiles = recordListCtrl.instance.is_change;

            ModStateManager.Instance.Present(
                Records.Where(r => r.Type == 0).Select(r => r.Name).ToList(),
                Records.Where(r => r.Type == 1).Select(r => r.Name).ToList(),
                canGoBack,
                canPresentProfiles);
        }

        /// <summary>
        /// Called when a record is added.
        /// </summary>
        [HarmonyPatch(typeof(recordListCtrl), nameof(recordListCtrl.addRecord))]
        [HarmonyPrefix]
        static void AddRecord(int in_id)
        {
            //Records are usually added all on the same frame unless it's new evidence.
            var noteData = piceDataCtrl.instance.note_data;

            if (in_id > noteData.Count)
            {
                Plugin.LogInfo($"Id {in_id} is not note data.");
                return;
            }

            var data = noteData[in_id];

            //Type 0 = Evidence
            //Type 1 = Profile

            //Plugin.LogInfo($"Added Record #{data.no}:{data.type}\r\n{data.name}\r\n{data.comment00} {data.comment01} {data.comment02}");
            var record = new Record
            {
                Id = in_id,
                Type = data.type,
                Name = data.name,
                Description = $"{data.comment00} {data.comment01} {data.comment02}".Trim()
            };

            Records.Add(record);
        }

        /// <summary>
        /// Called when a record is removed.
        /// </summary>
        //[HarmonyPatch(typeof(recordListCtrl), nameof(recordListCtrl.deleteRecord))]
        //[HarmonyPrefix]
        //static void DeleteRecord(int in_id)
        //{

        //}

        /// <summary>
        /// Called when a record is updated.
        /// </summary>
        //[HarmonyPatch(typeof(recordListCtrl), nameof(recordListCtrl.updateRecord))]
        //[HarmonyPrefix]
        //static void UpdateRecord(int in_id, int in_id2)
        //{

        //}

        public sealed class Record
        {
            public int Id { get; set; }
            public int Type { get; set; }
            public string Name { get; set; }
            public string Description { get; set; }
        }
    }
}
