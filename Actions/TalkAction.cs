using Newtonsoft.Json.Linq;
using Newtonsoft.Json.Schema;
using TurnaboutAI.NeuroAPI;
using TurnaboutAI.Utility;

namespace TurnaboutAI.Actions
{
    public sealed class TalkAction : IActionHandler
    {
        public CancellationToken CancellationToken { get; set; }

        public string Name => "talk";

        public string Description => "Talk to the person";

        public JsonSchema Schema => null;

        public void Execute(JObject data)
        {
            if(!CancellationToken.IsCancellationRequested)
            {
                ModStateManager.Instance.UnregisterAllActions();
                coroutineCtrl.instance.Play(new SequenceEnumerator(InvestigationMenuHelper.SelectOption(2)));
            }
        }

        public bool Validate(JObject data, out string message)
        {
            message = null;
            return true;
        }
    }
}
