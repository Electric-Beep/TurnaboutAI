using HarmonyLib;
using Newtonsoft.Json;
using System;
using System.IO;
using System.Runtime.InteropServices;
using System.Text;
using TurnaboutAI.SaveLoad;
using TurnaboutAI.Utility;
using UnityEngine;

namespace TurnaboutAI
{
    /// <summary>
    /// Starts the mod's main logic.
    /// </summary>
    public static class Plugin
    {
        internal static StreamWriter _logger;
        internal static IntPtr _stdHandle = IntPtr.Zero;
        internal readonly static Config Config;

        static Plugin()
        {
            Config = GetConfig();

            if(Config.LogToConsole && AllocConsole())
            {
                _stdHandle = GetStdHandle(-11);
            }

            _logger = new StreamWriter("TurnaboutAI.log");
            SaveLoadHelper.SetSlot(Config.SaveSlot);
        }

        /// <summary>
        /// Add the <see cref="ModStateManager"/> as a component to the game's main object.
        /// </summary>
        /// <param name="main"></param>
        public static void Init(GameObject main)
        {
            Plugin.LogInfo("Patching...");

            var harmony = new Harmony("com.ace.mod");
            harmony.PatchAll();

            Plugin.LogInfo("Patched.");

            main.AddComponent<ModStateManager>();
        }

        /// <summary>
        /// Gets the mod config.
        /// </summary>
        public static Config GetConfig()
        {
            const string ConfigFileName = "TurnaboutAIConfig.json";

            if (File.Exists(ConfigFileName))
            {
                string text = File.ReadAllText(ConfigFileName);
                var config = JsonConvert.DeserializeObject<Config>(text);

                return config ?? new Config();
            }

            return new Config();
        }

        /// <summary>
        /// Write an informative message to the log.
        /// </summary>
        public static void LogInfo(string message)
        {
            Log("info", message);
        }

        /// <summary>
        /// Write an error to the log.
        /// </summary>
        public static void LogError(object obj)
        {
            Log("error", obj.ToString());
        }

        /// <summary>
        /// Write an error to the log.
        /// </summary>
        public static void LogError(string message)
        {
            Log("error", message);
        }

        private static void Log(string level, string message)
        {
            string str = DateTime.Now.ToString() + '|' + level + '|' + message;
            WriteLine(str);
        }

        private static void WriteLine(string text)
        {
            try
            {

                if (_stdHandle != IntPtr.Zero)
                {
                    byte[] buffer = Encoding.UTF8.GetBytes(text + "\r\n");
                    WriteConsole(_stdHandle, buffer, buffer.Length, out int _, IntPtr.Zero);
                }

                _logger.WriteLine(text);
                _logger.Flush();
            }
            catch(Exception ex)
            {
                Debug.LogError(ex);
            }
        }

        [DllImport("kernel32.dll", SetLastError = true)]
        [return: MarshalAs(UnmanagedType.Bool)]
        static extern bool AllocConsole();

        [DllImport("kernel32.dll", SetLastError = true)]
        static extern IntPtr GetStdHandle([In] int nStdHandle);

        [DllImport("kernel32.dll", SetLastError = true)]
        [return: MarshalAs(UnmanagedType.Bool)]
        static extern bool WriteConsole(
            [In] IntPtr hConsoleOutput,
            [In] byte[] lpBuffer,
            [In] int nNumberOfCharsToWrite,
            [Out, Optional] out int lpNumberOfCharsWritten,
            IntPtr lpReserved);
    }
}
