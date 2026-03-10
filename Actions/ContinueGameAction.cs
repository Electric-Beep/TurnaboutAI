using Newtonsoft.Json.Linq;
using Newtonsoft.Json.Schema;
using System.Collections;
using TurnaboutAI.NeuroAPI;
using TurnaboutAI.Patches;
using TurnaboutAI.Utility;

namespace TurnaboutAI.Actions
{
    public sealed class ContinueGameAction : IActionHandler
    {
        public CancellationToken CancellationToken { get; set; }

        public string Name => "continue_game";

        public string Description => "Continues your last saved game.";

        public JsonSchema Schema => null;

        public void Execute(JObject data)
        {
            if(!CancellationToken.IsCancellationRequested)
            {
                ModStateManager.Instance.UnregisterAllActions();
                coroutineCtrl.instance.Play(new SequenceEnumerator(ContinueGame()));
            }
        }

        public bool Validate(JObject data, out string message)
        {
            message = null;
            return true;
        }

        private IEnumerator ContinueGame()
        {
            var selectPlate = MainTitlePatch.GetSelectPlate();

            while (selectPlate.cursor_no != 1)
            {
                yield return KeyPresser.PressAndWait(KeyType.Right, Waiter.ShortWait);
            }

            yield return Waiter.Wait(Waiter.ShortWait);

            const float WaitDur = 1f;

            KeyPresser.LongPress(KeyType.A, 2);
            yield return Waiter.Wait(WaitDur);

            KeyPresser.LongPress(KeyType.A, 2);
            yield return Waiter.Wait(WaitDur);

            KeyPresser.LongPress(KeyType.Left, 1);
            yield return Waiter.Wait(WaitDur);

            KeyPresser.LongPress(KeyType.A, 2);
            yield return Waiter.Wait(WaitDur);
        }
    }
}
