using TurnaboutAI.NeuroAPI;
using Newtonsoft.Json.Linq;
using Newtonsoft.Json.Schema;
using TurnaboutAI.Utility;

namespace TurnaboutAI.Actions
{
    /// <summary>
    /// Advances to the next dialogue.
    /// </summary>
    public sealed class NextDialogueAction : IActionHandler
    {
        public CancellationToken CancellationToken { get; set; }

        public string Name => "next_dialogue";

        public string Description => "Advance the dialogue.";

        public JsonSchema Schema => null;

        public void Execute(JObject data)
        {
            if(!CancellationToken.IsCancellationRequested)
            {
                ModStateManager.Instance.UnregisterAllActions();

                KeyPresser.PressKey(KeyType.A);
            }
        }

        public bool Validate(JObject data, out string message)
        {
            message = null;
            return true;
        }
    }
}
