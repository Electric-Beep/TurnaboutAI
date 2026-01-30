using TurnaboutAI.NeuroAPI;
using Newtonsoft.Json.Linq;
using Newtonsoft.Json.Schema;
using TurnaboutAI.Utility;

namespace TurnaboutAI.Actions
{
    /// <summary>
    /// Presses the B button basically.
    /// This typically will close whatever menu is opened.
    /// </summary>
    public sealed class CancelAction : IActionHandler
    {
        public CancellationToken CancellationToken { get; set; }

        public string Name => "cancel";

        public string Description => "Cancel the current action.";

        public JsonSchema Schema => null;

        public void Execute(JObject data)
        {
            if(!CancellationToken.IsCancellationRequested)
            {
                ModStateManager.Instance.UnregisterAllActions();

                KeyPresser.LongPress(KeyType.B, Waiter.ShortWait);
            }
        }

        public bool Validate(JObject data, out string message)
        {
            message = null;
            return true;
        }
    }
}
