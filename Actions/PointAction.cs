using TurnaboutAI.NeuroAPI;
using Newtonsoft.Json.Linq;
using Newtonsoft.Json.Schema;
using System.Collections;
using UnityEngine;
using TurnaboutAI.Utility;

namespace TurnaboutAI.Actions
{
    public sealed class PointAction : IActionHandler
    {
        private readonly GSPoint4 _thePoint;

        public PointAction(GSPoint4 thePoint)
        {
            _thePoint = thePoint;
        }

        public CancellationToken CancellationToken { get; set; }

        public string Name => "point_to";

        public string Description => "Points out the notable spot in the evidence.";

        public JsonSchema Schema => null;

        public void Execute(JObject data)
        {
            if(!CancellationToken.IsCancellationRequested)
            {
                ModStateManager.Instance.UnregisterAllActions();
                coroutineCtrl.instance.Play(new SequenceEnumerator(PointItOut(_thePoint)));
            }
        }

        public bool Validate(JObject data, out string message)
        {
            message = null;
            return true;
        }

        private static IEnumerator PointItOut(GSPoint4 point)
        {
            var polygon = new Polygon(point);
            var center = polygon.Centroid();
            var vector = new Vector3((float)center.X, (float)center.Y, 0);
            MiniGameCursor.instance.cursor_position = vector;

            yield return Waiter.Wait(Waiter.LongWait);

            KeyPresser.PressKey(KeyType.X);
        }
    }
}
