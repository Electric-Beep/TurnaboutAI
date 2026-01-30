using TurnaboutAI.NeuroAPI;
using Newtonsoft.Json.Linq;
using Newtonsoft.Json.Schema;
using System.Collections;
using System.Collections.Generic;
using TurnaboutAI.Utility;

namespace TurnaboutAI.Actions
{
    public sealed class MoveToAction : IActionHandler
    {
        private readonly List<string> _locations;

        public MoveToAction(List<string> locations)
        {
            _locations = locations;
        }

        public CancellationToken CancellationToken { get; set; }

        public string Name => "move_to";

        public string Description => "Select a location to move to.";

        public JsonSchema Schema => new JsonSchema
        {
            Type = JsonSchemaType.String,
            Properties = new Dictionary<string, JsonSchema>
            {
                { "location", Utilities.JEnum(_locations) }
            }
        };

        public void Execute(JObject data)
        {
            if(!CancellationToken.IsCancellationRequested)
            {
                string location = data?["location"]?.Value<string>();

                int index = _locations.IndexOf(location);

                Plugin.LogInfo($"Move To: {index} - {location}");

                if (index >= 0)
                {
                    ModStateManager.Instance.UnregisterAllActions();
                    coroutineCtrl.instance.Play(new SequenceEnumerator(MoveTo(index)));
                }
            }
        }

        public bool Validate(JObject data, out string message)
        {
            if(!CancellationToken.IsCancellationRequested)
            {
                string location = data?["location"]?.Value<string>();

                int index = _locations.IndexOf(location);

                if(index == -1)
                {
                    message = "Please choose a valid location.";
                    return false;
                }
            }

            message = null;
            return true;
        }

        private IEnumerator MoveTo(int index)
        {
            while (true)
            {
                int cursor = moveCtrl.instance.cursor_no;

                if(cursor < index)
                {
                    yield return KeyPresser.PressAndWait(KeyType.Down, Waiter.ShortWait);
                }
                else if(cursor > index)
                {
                    yield return KeyPresser.PressAndWait(KeyType.Up, Waiter.ShortWait);
                }
                else
                {
                    break;
                }
            }

            yield return Waiter.Wait(Waiter.ShortWait);
            KeyPresser.PressKey(KeyType.A);
        }
    }
}
