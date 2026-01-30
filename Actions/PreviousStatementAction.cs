using TurnaboutAI.NeuroAPI;
using Newtonsoft.Json.Linq;
using Newtonsoft.Json.Schema;

namespace TurnaboutAI.Actions
{
    /// <summary>
    /// Goes to the previous statement during cross examination.
    /// </summary>
    internal sealed class PreviousStatementAction : IActionHandler
    {
        public CancellationToken CancellationToken { get; set; }

        public string Name => "previous_statement";

        public string Description => "Go back to the previous statement.";

        public JsonSchema Schema => null;

        public void Execute(JObject data)
        {
            if(!CancellationToken.IsCancellationRequested)
            {
                ModStateManager.Instance.UnregisterAllActions();
                coroutineCtrl.instance.Play(KeyPresser.PressAndWait(KeyType.B, 0));
            }
        }

        public bool Validate(JObject data, out string message)
        {
            message = null;
            return true;
        }
    }
}
