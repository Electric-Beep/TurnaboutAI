using Newtonsoft.Json.Linq;
using Newtonsoft.Json.Schema;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace TurnaboutAI
{
    /// <summary>
    /// Helpers and utility functions.
    /// </summary>
    internal static class Utilities
    {
        public static void Do(IEnumerator enumerator, Action callback)
        {
            coroutineCtrl.instance.Play(DoImpl(enumerator, callback));
        }

        public static JsonSchema JEnum(IEnumerable<string> values)
        {
            return new JsonSchema
            {
                Enum = values.Select(s => new JValue(s)).Cast<JToken>().ToList()
            };
        }

        public static Texture2D CopyTexture(Texture2D source)
        {
            RenderTexture renderTex = RenderTexture.GetTemporary(
                source.width,
                source.height,
                0,
                RenderTextureFormat.Default,
                RenderTextureReadWrite.Linear);

            Graphics.Blit(source, renderTex);
            RenderTexture previous = RenderTexture.active;
            RenderTexture.active = renderTex;
            Texture2D readableText = new Texture2D(source.width, source.height);
            readableText.ReadPixels(new Rect(0, 0, renderTex.width, renderTex.height), 0, 0);
            readableText.Apply();
            RenderTexture.active = previous;
            RenderTexture.ReleaseTemporary(renderTex);
            return readableText;
        }

        private static IEnumerator DoImpl(IEnumerator enumerator, Action callback)
        {
            while (enumerator.MoveNext())
            {
                yield return enumerator.Current;
            }

            callback();
        }
    }
}
