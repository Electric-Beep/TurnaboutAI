using Newtonsoft.Json.Linq;
using Newtonsoft.Json.Schema;
using System.Collections;
using TurnaboutAI.NeuroAPI;
using TurnaboutAI.Patches;
using TurnaboutAI.Utility;
using UnityEngine;

namespace TurnaboutAI.Actions
{
    public sealed class NewGameAction : IActionHandler
    {
        public CancellationToken CancellationToken { get; set; }

        public string Name => "new_game";

        public string Description => "Start a new game.";

        public JsonSchema Schema => null;

        public void Execute(JObject data)
        {
            if(!CancellationToken.IsCancellationRequested)
            {
                ModStateManager.Instance.UnregisterAllActions();
                coroutineCtrl.instance.Play(new SequenceEnumerator(StartNewGame()));
            }
        }

        public bool Validate(JObject data, out string message)
        {
            message = null;
            return true;
        }

        private IEnumerator StartNewGame()
        {
            var selectPlate = MainTitlePatch.GetSelectPlate();

            while (selectPlate.cursor_no != 0)
            {
                yield return KeyPresser.PressAndWait(KeyType.Left, Waiter.ShortWait);
            }

            yield return Waiter.Wait(Waiter.ShortWait);

            for(int i = 0; i < 4; i++)
            {
                KeyPresser.LongPress(KeyType.A, 2);
                yield return Waiter.Wait(2);
            }
        }
    }
}
