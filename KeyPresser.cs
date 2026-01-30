using HarmonyLib;
using System.Collections;
using UnityEngine;

namespace TurnaboutAI
{
    /// <summary>
    /// Simulates key presses.
    /// </summary>
    [HarmonyPatch]
    public static class KeyPresser
    {
        private static KeyType _checkKey = KeyType.None;
        private static KeyType _pressedKey = KeyType.None;

        private static KeyCode _checkKeyCode = KeyCode.None;
        private static KeyCode _pressedKeyCode = KeyCode.None;

        private static float _longPressDur;
        private static int _longPressFrames;
        private static float _time;

        /// <summary>
        /// Presses a key and automatically releases it after a delay. Min 1 frame.
        /// </summary>
        /// <param name="keyType">Key to press.</param>
        /// <param name="duration">Press duration.</param>
        public static IEnumerator PressAndWait(KeyType keyType, float duration)
        {
            PressKey(keyType);

            return Waiter.Wait(duration);
        }

        /// <summary>
        /// Sets a key as pressed.
        /// </summary>
        public static void PressKey(KeyType keyType)
        {
            Plugin.LogInfo($"press: {keyType}");
            _pressedKey = keyType;
        }

        /// <summary>
        /// Sets a key as pressed.
        /// </summary>
        public static void PressKey(KeyCode keyCode)
        {
            Plugin.LogInfo($"press: {keyCode}");
            _pressedKeyCode = keyCode;
        }

        public static void LongPress(KeyType keyType, float duration)
        {
            _longPressDur = duration;
            _time = 0;
            _pressedKey = keyType;
        }

        public static void LongPress(KeyType keyType, int frames)
        {
            _longPressFrames = frames;
            _pressedKey = keyType;
        }

        /// <summary>
        /// Clears pressed key.
        /// </summary>
        public static void ReleaseKey()
        {
            if(_longPressDur != 0)
            {
                _time += Time.deltaTime;

                if(_time < _longPressDur)
                {
                    return;
                }

                _longPressDur = 0;
            }
            else if(_longPressFrames > 0)
            {
                _longPressFrames--;
                return;
            }

            _pressedKey = KeyType.None;
            _pressedKeyCode = KeyCode.None;
        }

        [HarmonyPatch(typeof(padCtrl), nameof(padCtrl.GetKey))]
        [HarmonyPrefix]
        static void GetKey(KeyType type, int ext = 2, bool is_debug = true, KeyType is_controller_type = KeyType.None)
        {
            _checkKey = type;
        }

        [HarmonyPatch(typeof(padCtrl), nameof(padCtrl.GetKey))]
        [HarmonyPostfix]
        static void GetKey(ref bool __result)
        {
            if(_pressedKey != KeyType.None && _checkKey != KeyType.None)
            {
                __result = _pressedKey == _checkKey;
            }
        }

        [HarmonyPatch(typeof(padCtrl), nameof(padCtrl.GetKeyDown))]
        [HarmonyPrefix]
        static void GetKeyDown(KeyType type, int ext = 2, bool is_debug = true, KeyType is_controller_type = KeyType.None)
        {
            _checkKey = type;
        }

        [HarmonyPatch(typeof(padCtrl), nameof(padCtrl.GetKeyDown))]
        [HarmonyPostfix]
        static void GetKeyDown(ref bool __result)
        {
            if (_pressedKey != KeyType.None && _checkKey != KeyType.None)
            {
                __result = _pressedKey == _checkKey;
            }
        }

        [HarmonyPatch(typeof(padCtrl), nameof(padCtrl.InputGetKeyDown))]
        [HarmonyPrefix]
        static void InputGetKeyDown(KeyCode _keycode)
        {
            _checkKeyCode = _keycode;
        }

        [HarmonyPatch(typeof(padCtrl), nameof(padCtrl.InputGetKeyDown))]
        [HarmonyPostfix]
        static void InputGetKeyDown(ref bool __result)
        {
            if(_pressedKeyCode != KeyCode.None && _checkKeyCode != KeyCode.None)
            {
                __result = _pressedKeyCode == _checkKeyCode;
            }
        }
    }
}
