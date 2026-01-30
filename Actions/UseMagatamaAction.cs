using TurnaboutAI.NeuroAPI;
using TurnaboutAI.Patches;
using Newtonsoft.Json.Linq;
using Newtonsoft.Json.Schema;
using System.Collections;
using System.Linq;

namespace TurnaboutAI.Actions
{
    public sealed class UseMagatamaAction : IActionHandler
    {
        public CancellationToken CancellationToken { get; set; }

        public string Name => "use_magatama";

        public string Description => "Use the magatama to attempt to break psyche locks.";

        public JsonSchema Schema => null;

        public void Execute(JObject data)
        {
            if(!CancellationToken.IsCancellationRequested)
            {
                ModStateManager.Instance.UnregisterAllActions();
                ModStateManager.Instance.SuppressEvents = true;
                coroutineCtrl.instance.Play(new SequenceEnumerator(PerformAction()));
            }
        }

        public bool Validate(JObject data, out string message)
        {
            message = null;
            return true;
        }

        private IEnumerator PerformAction()
        {
            yield return Back();
            yield return SelectOption(3);

            while(!recordListCtrl.instance.is_open)
            {
                yield return null;
            }

            ModStateManager.Instance.SuppressEvents = false;

            int evidenceIndex = RecordPatch.GetEvidence().ToList().FindIndex(r => r.Name.Contains("Magatama"));

            yield return PresentEvidence(evidenceIndex);
        }

        private IEnumerator Back()
        {
            KeyPresser.LongPress(KeyType.B, Waiter.ShortWait);
            yield return Waiter.Wait(Waiter.ShortWait);

            while(!selectPlateCtrl.instance.is_end)
            {
                yield return null;
            }
        }

        private IEnumerator SelectOption(int index)
        {
            tanteiMenu.instance.cursor_no = index;

            yield return Waiter.Wait(Waiter.ShortWait);
            KeyPresser.LongPress(KeyType.A, Waiter.ShortWait);
        }

        private IEnumerator PresentEvidence(int index)
        {
            recordListCtrl.instance.record_data_[recordListCtrl.instance.record_type].cursor_no_ = index;

            yield return Waiter.Wait(Waiter.ShortWait);
            KeyPresser.LongPress(KeyType.X, Waiter.ShortWait);
        }
    }
}
