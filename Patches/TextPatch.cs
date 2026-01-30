using HarmonyLib;
using System;
using System.Collections.Generic;
using System.Text;
using System.Text.RegularExpressions;

namespace TurnaboutAI.Patches
{
    /// <summary>
    /// Handler for the dialogue box.
    /// </summary>
    [HarmonyPatch]
    internal static class TextPatch
    {
        private readonly static string[] GS1NameMap = new string[55]
        {
            string.Empty, "???", "Phoenix", "Police", "Maya",
            "Mia", "Alarm Clock", "Mia", "Judge", "Edgeworth",
            "Payne", "Interphone", "Grossberg", "Cellular", "Public",
            "???", "Penny", "Oldbag", "Manella", "TV",
            "Gumshoe", "White", "April", "Bellboy", "Vasquez",
            "Butz", "Sahwit", "Will", "Cody", "Phoenix",
            "Edgeworth", "Lotta", "Yogi", "Karma", "Parrot",
            "Missile", "Caretaker", "Bailiff", "Teacher", "Edgeworth",
            "Butz", string.Empty, string.Empty, "Chief", "Ema",
            "Lana", "Marshall", "Meekins", "Goodman", "Gant",
            "Angel", "Guard", "Officer", "Patrolman", string.Empty
        };

        private readonly static string[] GS2NameMap = new string[55]
        {
            "Phoenix", "???", "???", "Phoenix", "Maya",
            "Mia", "Mia", "Judge", "Edgeworth", "Payne",
            "Gumshoe", "Phone", "Public", "Bailiff", "von Karma",
            "von Karma", "Wellington", "Byrde", "Byrde", "Ini",
            "Pearl", "Morgan", "Hotti", "Grey", "Lotta",
            "Mia", "Nurse", "Mimi", "Regina", "Max",
            "Ben", "Moe", "Acro", "Trilo", "Money",
            "Engarde", "Andrews", "de Killer", "de Killer", "Oldbag",
            "Powers", "TV", string.Empty, string.Empty, string.Empty,
            "Celeste", "Russell", "Bellboy", "PA Notice", "Ray Gun",
            "Chief", "Detective", "Guard", "Shoe", "Doe"
        };

        private readonly static string[] GS3NameMap = new string[55]
        {
            "Phoenix", "Mia", "Grossberg", "Payne", "Judge",
            "Dahlia", "???", "Swallow", "Judge", "Ron",
            "Desirée", "Demasque", "Andrews", "Godot", "Atmey",
            "Pearl", "Announcer", "Phone", "Gumshoe", "Butz",
            "Maya", "Bailiff", "Officer", "Buzzer", "Armstrong",
            "Basil", "Viola", "Tigre", "Byrde", "Kudo",
            "Chief", "Old Man", "Detective", "The Tiger", "Programmer",
            "Valerie", "Fawles", "Edgeworth", "Melissa", "Armando",
            "Bikini", "Iris", "Laurice", "Elise", "von Karma",
            "Morgan", "Misty", "Oldbag", "Ray Gun", string.Empty,
            "Violetta", string.Empty, string.Empty, string.Empty, string.Empty
        };

        private readonly static int[] GS3NameIdTable = new int[69]
        {
            49, 6, 6, 0, 20, 4, 18, 1, 39, 3,
            0, 5, 9, 14, 10, 19, 12, 13, 28, 24,
            27, 26, 1, 29, 25, 47, 37, 36, 43, 19,
            15, 49, 40, 41, 44, 45, 0, 1, 49, 49,
            49, 1, 2, 1, 20, 49, 1, 17, 21, 8,
            7, 16, 11, 22, 23, 49, 49, 13, 31, 30,
            32, 33, 34, 38, 35, 42, 49, 46, 48
        };

        /// <summary>
        /// If false, when the message system starts up, the last text from the save will
        /// be read and sent as context.
        /// </summary>
        public static bool Init = false;

        private static readonly LinkedList<Dialogue> Dialogues = new LinkedList<Dialogue>();

        [HarmonyPatch(typeof(MessageSystem), "SetCharacter", new Type[] { typeof(MessageWork), typeof(char) })]
        [HarmonyPrefix]
        static void SetCharacter(MessageWork message_work, char character)
        {
            Dialogue dialogue;

            if(Dialogues.Count == 0)
            {
                dialogue = new Dialogue
                {
                    SpeakerId = message_work.speaker_id,
                };

                Dialogues.AddLast(dialogue);
            }
            else
            {
                dialogue = Dialogues.Last.Value;
            }

            if (message_work.speaker_id != dialogue.SpeakerId)
            {
                dialogue = new Dialogue
                {
                    SpeakerId = message_work.speaker_id,
                };

                Dialogues.AddLast(dialogue);
            }

            if (message_work.message_line != dialogue.LastLineNo)
            {
                dialogue.Buffer.Append(' ');
                dialogue.LastLineNo = message_work.message_line;
            }

            ModStateManager.Instance.TextStarted();

            string translated = MessageSystem.EnToHalf(character.ToString(), GSStatic.global_work_.language);

            dialogue.Buffer.Append(translated);
        }

        [HarmonyPatch(typeof(MessageSystem), nameof(MessageSystem.Message_main))]
        [HarmonyPrefix]
        static void MessageMain()
        {
            bool arrowLActive = messageBoardCtrl.instance.arrowL.active;
            bool arrowRActive = messageBoardCtrl.instance.arrowR.active;
            var guideType = messageBoardCtrl.instance.guide_ctrl.current_guide;

            if (!Init)
            {
                Init = true;
                string curMsg = ReadMessageFromSave();

                if (!string.IsNullOrEmpty(curMsg))
                {
                    ModStateManager.Instance.TextFinished(curMsg, arrowLActive, guideType);
                }

                messageBoardCtrl.instance.guide_ctrl.changeGuide(messageBoardCtrl.instance.guide_ctrl.GetChangeGuideType());
            }

            MessageWork activeMessage = MessageSystem.GetActiveMessageWork();

            if (activeMessage.mdt_data == null)
            {
                return;
            }

            bool wait = (activeMessage.status & MessageSystem.Status.RT_WAIT) == MessageSystem.Status.RT_WAIT;
            bool loop = (activeMessage.status & MessageSystem.Status.LOOP) == MessageSystem.Status.LOOP;

            if ((arrowLActive || arrowRActive) && Dialogues.Count > 0 && (wait || loop))
            {
                string msg = ConsumeDialogues();

                if (!string.IsNullOrEmpty(msg))
                {
                    ModStateManager.Instance.TextFinished(msg, arrowLActive, guideType);
                }
            }
        }

        public static string ConsumeDialogues()
        {
            StringBuilder context = new StringBuilder();
            foreach (var dialogue in Dialogues)
            {
                string text = dialogue.Buffer.ToString();

                if (string.IsNullOrEmpty(text))
                {
                    continue;
                }

                if(context.Length > 0)
                {
                    context.AppendLine();
                }

                string speakerName = GetSpeakerName(dialogue.SpeakerId);

                if(!string.IsNullOrEmpty(speakerName))
                {
                    context.Append(speakerName).Append(": ").Append(text);
                }
                else
                {
                    context.Append('[').Append(text).Append(']');
                }
            }

            Dialogues.Clear();
            return context.ToString();
        }

        private static string GetSpeakerName(int speakerId)
        {
            switch(GSStatic.global_work_.title)
            {
                case TitleId.GS1:
                    return GS1NameMap[speakerId];
                case TitleId.GS2:
                    return GS2NameMap[speakerId];
                case TitleId.GS3:
                    return GS3NameMap[GS3NameIdTable[speakerId]];
            }

            return string.Empty;
        }

        private static string ReadMessageFromSave()
        {
            if(GSStatic.msg_save_data != null)
            {
                string text = $"{GSStatic.msg_save_data.msg_line1} {GSStatic.msg_save_data.msg_line2} {GSStatic.msg_save_data.msg_line3}".Trim();
                text = Regex.Replace(text, "<[^>]+>", string.Empty);

                if (string.IsNullOrEmpty(text)) return null;

                string speaker = GetSpeakerName(GSStatic.msg_save_data.name_no);

                if(!string.IsNullOrEmpty(speaker))
                {
                    return $"{speaker}: {text}";
                }
                else
                {
                    return $"[{text}]";
                }
            }

            return string.Empty;
        }

        private sealed class Dialogue
        {
            public int SpeakerId { get; set; }
            public int LastLineNo { get; set; }
            public StringBuilder Buffer { get; } = new StringBuilder();
        }
    }
}
