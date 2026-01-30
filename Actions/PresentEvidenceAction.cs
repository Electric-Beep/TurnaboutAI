using TurnaboutAI.NeuroAPI;
using Newtonsoft.Json.Linq;
using Newtonsoft.Json.Schema;
using System.Collections;
using System.Collections.Generic;
using TurnaboutAI.Utility;

namespace TurnaboutAI.Actions
{
    /// <summary>
    /// Navigates to the chosen piece of evidence in the present window and selects it.
    /// </summary>
    internal sealed class PresentEvidenceAction : IActionHandler
    {
        private readonly List<string> _evidence;

        public PresentEvidenceAction(List<string> evidence)
        {
            _evidence = evidence;
        }

        public CancellationToken CancellationToken { get; set; }

        public string Name => "present_evidence";

        public string Description => "Present a piece of evidence.";

        public JsonSchema Schema => new JsonSchema
        {
            Type = JsonSchemaType.Object,
            Properties = new Dictionary<string, JsonSchema>
            {
                { "evidence", Utilities.JEnum(_evidence) }
            }
        };

        public void Execute(JObject data)
        {
            if(!CancellationToken.IsCancellationRequested)
            {
                string evidence = data?["evidence"]?.Value<string>();

                int index = _evidence.IndexOf(evidence);

                Plugin.LogInfo($"chosen: {evidence} - {index}");

                if (index != -1)
                {
                    ModStateManager.Instance.UnregisterAllActions();
                    coroutineCtrl.instance.Play(new SequenceEnumerator(PresentEvidence(index)));
                }
            }
        }

        public bool Validate(JObject data, out string message)
        {
            if(CancellationToken.IsCancellationRequested)
            {
                message = null;
                return true;
            }

            string evidence = data?["evidence"]?.Value<string>();

            if(_evidence.Contains(evidence))
            {
                message = null;
                return true;
            }

            message = "Please pick a valid piece of evidence";
            return false;
        }

        private IEnumerator PresentEvidence(int index)
        {
            while(true)
            {
                int curIndex = recordListCtrl.instance.record_data_[recordListCtrl.instance.record_type].cursor_no_;

                curIndex += (10 * advCtrl.instance.sub_window_.note_.item_page);

                if(curIndex < index)
                {
                    yield return KeyPresser.PressAndWait(KeyType.Right, Waiter.ShortWait);
                }
                else if(curIndex > index)
                {
                    yield return KeyPresser.PressAndWait(KeyType.Left, Waiter.ShortWait);
                }
                else
                {
                    break;
                }
            }

            yield return Waiter.Wait(Waiter.ShortWait);
            KeyPresser.PressKey(KeyType.X);
        }
    }
}
