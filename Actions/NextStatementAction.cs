using TurnaboutAI.NeuroAPI;
using Newtonsoft.Json.Linq;
using Newtonsoft.Json.Schema;
using TurnaboutAI.Utility;

namespace TurnaboutAI.Actions
{
    /// <summary>
    /// Advances to the next statement during cross examination.
    /// </summary>
    internal sealed class NextStatementAction : IActionHandler
    {
        public CancellationToken CancellationToken { get; set; }

        public string Name => "next_statement";

        public string Description => "Advance to the next statement.";

        public JsonSchema Schema => null;

        public void Execute(JObject data)
        {
            if(!CancellationToken.IsCancellationRequested)
            {
                ModStateManager.Instance.UnregisterAllActions();
                coroutineCtrl.instance.Play(KeyPresser.PressAndWait(KeyType.A, 0));
            }
        }

        public bool Validate(JObject data, out string message)
        {
            message = null;
            return true;
        }
    }
}
