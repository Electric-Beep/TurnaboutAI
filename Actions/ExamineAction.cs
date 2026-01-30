using Newtonsoft.Json.Linq;
using Newtonsoft.Json.Schema;
using TurnaboutAI.NeuroAPI;
using TurnaboutAI.Utility;

namespace TurnaboutAI.Actions
{
    public sealed class ExamineAction : IActionHandler
    {
        public CancellationToken CancellationToken { get; set; }

        public string Name => "examine";

        public string Description => "Examine the area for clues.";

        public JsonSchema Schema => null;

        public void Execute(JObject data)
        {
            if(!CancellationToken.IsCancellationRequested)
            {
                ModStateManager.Instance.UnregisterAllActions();
                coroutineCtrl.instance.Play(new SequenceEnumerator(InvestigationMenuHelper.SelectOption(0)));
            }
        }

        public bool Validate(JObject data, out string message)
        {
            message = null;
            return true;
        }
    }
}
