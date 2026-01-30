using TurnaboutAI.NeuroAPI;
using Newtonsoft.Json.Linq;
using Newtonsoft.Json.Schema;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace TurnaboutAI.Actions
{
    /// <summary>
    /// Gives a list of spots that can be picked during Inspect sections.
    /// </summary>
    public sealed class ExamineSpotAction : IActionHandler
    {
        private readonly List<ExamineSpot> _spots;

        public ExamineSpotAction(List<ExamineSpot> spots)
        {
            _spots = spots;
        }

        public CancellationToken CancellationToken { get; set; }

        public string Name => "examine_spot";

        public string Description => "Examines an interesting spot.";

        public JsonSchema Schema => new JsonSchema
        {
            Type = JsonSchemaType.Object,
            Properties = new Dictionary<string, JsonSchema>
            {
                { "spot", Utilities.JEnum(_spots.Select(s => s.Name)) }
            }
        };

        public void Execute(JObject data)
        {
            string spot = data?["spot"]?.Value<string>();

            var obj = _spots.FirstOrDefault(s => s.Name == spot);

            if (obj != null)
            {
                ModStateManager.Instance.UnregisterAllActions();
                coroutineCtrl.instance.Play(new SequenceEnumerator(PickSpot(obj)));
            }
        }

        public bool Validate(JObject data, out string message)
        {
            string spot = data?["spot"]?.Value<string>();

            var obj = _spots.FirstOrDefault(s => s.Name == spot);

            if(obj == null)
            {
                message = "Please choose a valid option.";
                return false;
            }

            message = null;
            return true;
        }

        private IEnumerator PickSpot(ExamineSpot spot)
        {
            Plugin.LogInfo($"Picked Spot: {spot.Name} {spot.Polygon}");

            while(true)
            {
                if (bgCtrl.instance.is_slider)
                {
                    yield return null;
                    continue;
                }

                var polygon = spot.Polygon;

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

                //Despite the inspect data being in 1920x1080, the coordinates are 1800x960.
                polygon = polygon.Scale(1800d / 1920d, 960d / 1080d);
                var center = polygon.Centroid();

                center = center.Translate(-900, -480); 
                center = center.Scale(1, -1); //The coordinate system is flipped on Y.

                //The extra is to align the center of the select sprite to the center of the box.
                center = center.Translate(0, -40);

                Plugin.LogInfo($"Setting Cursor To: {center}");

                inspectCtrl.instance.SetCursorPos((float)center.X, (float)center.Y);
                yield return Waiter.Wait(Waiter.ShortWait);
                KeyPresser.PressKey(KeyType.A);
                break;
            }
        }
    }
}
