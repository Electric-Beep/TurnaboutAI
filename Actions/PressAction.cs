using TurnaboutAI.NeuroAPI;
using Newtonsoft.Json.Linq;
using Newtonsoft.Json.Schema;

namespace TurnaboutAI.Actions
{
    /// <summary>
    /// Does a Press during cross examination.
    /// </summary>
    internal sealed class PressAction : IActionHandler
    {
        public CancellationToken CancellationToken { get; set; }

        public string Name => "press";

        public string Description => "Press the witness on their current statement to try to get more information out of them.";

        public JsonSchema Schema => null;

        public void Execute(JObject data)
        {
           if(!CancellationToken.IsCancellationRequested)
            {
                ModStateManager.Instance.UnregisterAllActions();
                coroutineCtrl.instance.Play(KeyPresser.PressAndWait(KeyType.L, 0));
            }
        }

        public bool Validate(JObject data, out string message)
        {
            message = null;
            return true;
        }
    }
}
