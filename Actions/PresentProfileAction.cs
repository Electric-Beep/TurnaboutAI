using TurnaboutAI.NeuroAPI;
using Newtonsoft.Json.Linq;
using Newtonsoft.Json.Schema;
using System.Collections;
using System.Collections.Generic;

namespace TurnaboutAI.Actions
{
    /// <summary>
    /// Swaps to profiles in the present window, navigates to the chosen profile, and selects it.
    /// </summary>
    internal sealed class PresentProfileAction : IActionHandler
    {
        private readonly List<string> _profiles;

        public PresentProfileAction(List<string> profiles)
        {
            _profiles = profiles;
        }

        public CancellationToken CancellationToken { get; set; }

        public string Name => "present_profile";

        public string Description => "Present a psychological profile.";

        public JsonSchema Schema => new JsonSchema
        {
            Type = JsonSchemaType.Object,
            Properties = new Dictionary<string, JsonSchema>
            {
                { "profile", Utilities.JEnum(_profiles) }
            }
        };

        public void Execute(JObject data)
        {
            if (!CancellationToken.IsCancellationRequested)
            {
                string profile = data?["profile"]?.Value<string>();

                int index = _profiles.IndexOf(profile);

                if (index != -1)
                {
                    ModStateManager.Instance.UnregisterAllActions();
                    coroutineCtrl.instance.Play(new SequenceEnumerator(PresentProfile(index)));
                }
            }
        }

        public bool Validate(JObject data, out string message)
        {
            if (CancellationToken.IsCancellationRequested)
            {
                message = null;
                return true;
            }

            string profile = data?["profile"]?.Value<string>();

            if (_profiles.Contains(profile))
            {
                message = null;
                return true;
            }

            message = "Please pick a valid profile";
            return false;
        }

        private IEnumerator PresentProfile(int index)
        {
            while (true)
            {
                while(recordListCtrl.instance.is_page_changing)
                {
                    yield return null;
                }

                if(recordListCtrl.instance.record_type != 1)
                {
                    yield return KeyPresser.PressAndWait(KeyType.Record, Waiter.ShortWait);
                    continue;
                }

                int curIndex = recordListCtrl.instance.record_data_[recordListCtrl.instance.record_type].cursor_no_;

                curIndex += (10 * advCtrl.instance.sub_window_.note_.man_page);

                if (curIndex < index)
                {
                    yield return KeyPresser.PressAndWait(KeyType.Right, Waiter.ShortWait);
                }
                else if (curIndex > index)
                {
                    yield return KeyPresser.PressAndWait(KeyType.Left, Waiter.ShortWait);
                }
                else
                {
                    break;
                }
            }

            yield return Waiter.Wait(Waiter.ShortWait);
            KeyPresser.PressKey(KeyType.X);
        }
    }
}
