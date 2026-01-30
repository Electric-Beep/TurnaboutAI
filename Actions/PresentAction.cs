using TurnaboutAI.NeuroAPI;
using Newtonsoft.Json.Linq;
using Newtonsoft.Json.Schema;

namespace TurnaboutAI.Actions
{
    /// <summary>
    /// Opens the present window.
    /// </summary>
    internal sealed class PresentAction : IActionHandler
    {
        private readonly bool _inCourt;

        public PresentAction(bool inCourt)
        {
            _inCourt = inCourt;
        }

        public CancellationToken CancellationToken { get; set; }

        public string Name => "present";

        public string Description
        {
            get
            {
                if(_inCourt) return "Present a piece of evidence or a psychological profile that contradicts the witness's current statement.";

                return "Present evidence or psychological profile to the person to see if they know anything.";
            }
        }

        public JsonSchema Schema => null;

        public void Execute(JObject data)
        {
            if(!CancellationToken.IsCancellationRequested)
            {
                ModStateManager.Instance.UnregisterAllActions();

                if(_inCourt)
                {
                    coroutineCtrl.instance.Play(KeyPresser.PressAndWait(KeyType.R, 0));
                }
                else
                {
                    coroutineCtrl.instance.Play(new SequenceEnumerator(InvestigationMenuHelper.SelectOption(3)));
                }
            }
        }

        public bool Validate(JObject data, out string message)
        {
            message = null;
            return true;
        }
    }
}
