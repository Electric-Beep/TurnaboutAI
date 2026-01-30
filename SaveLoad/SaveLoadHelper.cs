using SaveStruct;
using System;
using System.Collections;

namespace TurnaboutAI.SaveLoad
{
    /// <summary>
    /// Save logic helper.
    /// </summary>
    internal static class SaveLoadHelper
    {
        private static int SaveSlotNo = 0;

        public static void SetSlot(int slot)
        {
            if (slot < 0 || slot > 9) return;
            SaveSlotNo = slot;
        }

        public static ModSaveData GetSaveData()
        {
            int slotNo = GSUtility.GetLanguageSlotNum(SaveSlotNo, GSStatic.global_work_.language);

            string time = DateTime.Now.ToString("yyyy/MM/dd\nHH:mm:ss");

            var tempSave = SaveTemp(time);

            PresideData.New(out PresideData preside);
            preside.system_data_.CopyFromStatic();

            GetTempData();
            GameData[] tempGameData = GSStatic.game_data_temp_;

            GameData.New(out tempGameData[slotNo]);
            tempGameData[slotNo].CopyFromStatic();
            tempGameData.CopyTo(preside.slot_list_, 0);
            byte[] data = StructSerializer.Serialize(ref preside);

            RestoreTemp(tempSave);

            return new ModSaveData
            {
                SaveDataBytes = data,
                SaveData = new SaveData
                {
                    time = time,
                    title = (ushort)GSStatic.global_work_.title,
                    scenario = GSStatic.global_work_.story,
                    progress = GSStatic.global_work_.scenario,
                    in_data = 1,
                }
            };
        }

        public static IEnumerator DoSave(ModSaveData saveData)
        {
            Plugin.LogInfo("!!! SAFETY SAVE !!!");

            SaveData saveTemp = new SaveData
            {
                time = GSStatic.save_data[SaveSlotNo].time,
                title = GSStatic.save_data[SaveSlotNo].title,
                scenario = GSStatic.save_data[SaveSlotNo].scenario,
                progress = GSStatic.save_data[SaveSlotNo].progress,
                in_data = GSStatic.save_data[SaveSlotNo].in_data
            };

            GSStatic.save_data[SaveSlotNo].time = saveData.SaveData.time;
            GSStatic.save_data[SaveSlotNo].title = saveData.SaveData.title;
            GSStatic.save_data[SaveSlotNo].scenario = saveData.SaveData.scenario;
            GSStatic.save_data[SaveSlotNo].progress = saveData.SaveData.progress;
            GSStatic.save_data[SaveSlotNo].in_data = saveData.SaveData.in_data;

            SaveDataAccessor.SaveRequest(SaveControl.GetSystemDataFileName(), saveData.SaveDataBytes);

            while (!SaveControl.is_save_)
            {
                yield return null;
            }

            if (SaveControl.is_save_error)
            {
                Plugin.LogError("!!! SAVE ERROR !!!");

                RestoreTemp(saveTemp);
            }
        }

        private static SaveData SaveTemp(string time)
        {
            SaveData saveTemp = new SaveData
            {
                time = GSStatic.save_data[SaveSlotNo].time,
                title = GSStatic.save_data[SaveSlotNo].title,
                scenario = GSStatic.save_data[SaveSlotNo].scenario,
                progress = GSStatic.save_data[SaveSlotNo].progress,
                in_data = GSStatic.save_data[SaveSlotNo].in_data
            };

            GSStatic.save_data[SaveSlotNo].time = time;
            GSStatic.save_data[SaveSlotNo].title = (ushort)GSStatic.global_work_.title;
            GSStatic.save_data[SaveSlotNo].scenario = GSStatic.global_work_.story;
            GSStatic.save_data[SaveSlotNo].progress = GSStatic.global_work_.scenario;
            GSStatic.save_data[SaveSlotNo].in_data = 1;

            return saveTemp;
        }

        private static void RestoreTemp(SaveData temp)
        {
            GSStatic.save_data[SaveSlotNo].time = temp.time;
            GSStatic.save_data[SaveSlotNo].title = temp.title;
            GSStatic.save_data[SaveSlotNo].scenario = temp.scenario;
            GSStatic.save_data[SaveSlotNo].progress = temp.progress;
            GSStatic.save_data[SaveSlotNo].in_data = temp.in_data;
        }

        private static void GetTempData()
        {
            PresideData.New(out PresideData preside);

            byte[] tempData = SaveDataAccessor.Load(SaveControl.GetSystemDataFileName());

            if (tempData != null)
            {
                StructSerializer.Deserialize(ref preside, tempData);
                preside.slot_list_.CopyTo(GSStatic.game_data_temp_, 0);
            }
        }
    }
}
