using System.Collections;
using UnityEngine;

namespace TurnaboutAI.Utility
{
    /// <summary>
    /// Helper class to wait for a duration.
    /// </summary>
    internal static class Waiter
    {
        public const float ShortWait = 0.3f;
        public const float LongWait = 1f;

        /// <summary>
        /// Wait for a duration.
        /// </summary>
        /// <param name="seconds">Number of seconds to wait.</param>
        public static IEnumerator Wait(float seconds)
        {
            float time = 0;
            while (true)
            {
                time += Time.deltaTime;

                if (time >= seconds)
                {
                    break;
                }

                yield return null;
            }
        }
    }
}
