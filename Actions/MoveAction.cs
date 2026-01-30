using Newtonsoft.Json.Linq;
using Newtonsoft.Json.Schema;
using TurnaboutAI.NeuroAPI;
using TurnaboutAI.Utility;

namespace TurnaboutAI.Actions
{
    public sealed class MoveAction : IActionHandler
    {
        public CancellationToken CancellationToken { get; set; }

        public string Name => "move";

        public string Description => "Move to a different area.";

        public JsonSchema Schema => null;

        public void Execute(JObject data)
        {
            if(!CancellationToken.IsCancellationRequested)
            {
                ModStateManager.Instance.UnregisterAllActions();
                coroutineCtrl.instance.Play(new SequenceEnumerator(InvestigationMenuHelper.SelectOption(1)));
            }
        }

        public bool Validate(JObject data, out string message)
        {
            message = null;
            return true;
        }
    }
}
