using TurnaboutAI.NeuroAPI;
using TurnaboutAI.Patches;
using Newtonsoft.Json.Linq;
using Newtonsoft.Json.Schema;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace TurnaboutAI.Actions
{
    public sealed class DetectSpotAction : IActionHandler
    {
        private readonly List<DetectPoint> _points;

        public DetectSpotAction(List<DetectPoint> points)
        {
            _points = points;
        }

        public CancellationToken CancellationToken { get; set; }

        public string Name => "detect_spot";

        public string Description => "Use the tool on a detection spot.";

        public JsonSchema Schema => new JsonSchema
        {
            Type = JsonSchemaType.Object,
            Properties = new Dictionary<string, JsonSchema>
            {
                { "spot", Utilities.JEnum(_points.Select(p => p.Name)) }
            }
        };

        public void Execute(JObject data)
        {
            if(!CancellationToken.IsCancellationRequested)
            {
                string spot = data?["spot"]?.Value<string>();

                DetectPoint point = _points.FirstOrDefault(p => p.Name == spot);

                if(point != null)
                {
                    ModStateManager.Instance.UnregisterAllActions();
                    TanchikiPatch.FlagChecked(point.Index);
                    coroutineCtrl.instance.Play(new SequenceEnumerator(Detect(point)));
                }
            }
        }

        public bool Validate(JObject data, out string message)
        {
            if(!CancellationToken.IsCancellationRequested)
            {
                string spot = data?["spot"]?.Value<string>();

                DetectPoint point = _points.FirstOrDefault(p => p.Name == spot);

                if(point == null)
                {
                    message = "Please choose a valid option";
                    return false;
                }
            }

            message = null;
            return true;
        }

        private IEnumerator Detect(DetectPoint point)
        {
            Plugin.LogInfo($"Picked Spot: {point.Name}");

            while(true)
            {
                if (bgCtrl.instance.is_slider)
                {
                    yield return null;
                    continue;
                }

                Polygon polygon = new Polygon(point.HitBox);

                double minX = polygon.MinX;

                if ((minX >= 1920 && bgCtrl.instance.bg_pos_x < 1920) || (minX < 1920 && bgCtrl.instance.bg_pos_x >= 1920))
                {
                    Plugin.LogInfo("Slide Screen.");

                    KeyPresser.PressKey(KeyType.L);

                    yield return null;
                    continue;
                }

                if (minX > 1920)
                {
                    polygon = polygon.Translate(-1920, 0);
                }

                var center = polygon.Centroid();

                Plugin.LogInfo($"Setting Cursor To: {center}");

                MiniGameCursor.instance.cursor_position = new UnityEngine.Vector3((float)center.X, (float)center.Y, 0);

                yield return Waiter.Wait(Waiter.LongWait);

                KeyPresser.PressKey(KeyType.A);
                break;
            }
        }
    }
}
