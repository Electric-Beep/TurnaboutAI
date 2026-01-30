using TurnaboutAI.NeuroAPI;
using TurnaboutAI.Patches;
using Newtonsoft.Json.Linq;
using Newtonsoft.Json.Schema;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace TurnaboutAI.Actions
{
    /// <summary>
    /// Selects an option during uncancelable multiple choice sections.
    /// </summary>
    public sealed class PickChoiceAction : IActionHandler
    {
        private readonly List<TalkOption> _options;

        public PickChoiceAction(List<TalkOption> options)
        {
            _options = options;
        }

        public CancellationToken CancellationToken { get; set; }

        public string Name => "pick_choice";

        public string Description => "Pick a choice.";

        public JsonSchema Schema => new JsonSchema
        {
            Type = JsonSchemaType.Object,
            Properties = new Dictionary<string, JsonSchema>
            {
                { "choice", Utilities.JEnum(_options.Select(o => o.Text)) }
            }
        };

        public void Execute(JObject data)
        {
            if (!CancellationToken.IsCancellationRequested)
            {
                ModStateManager.Instance.UnregisterAllActions();

                string choice = data?["choice"]?.Value<string>();

                var option = _options.FirstOrDefault(o => o.Text == choice);

                if (option != null)
                {
                    int index = _options.IndexOf(option);

                    coroutineCtrl.instance.Play(new SequenceEnumerator(SelectOption(index)));
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

            string choice = data?["choice"]?.Value<string>();

            if (string.IsNullOrEmpty(choice) || !_options.Any(o => o.Text == choice))
            {
                message = "Please choose a valid choice.";
                return false;
            }

            message = string.Empty;
            return true;
        }

        private IEnumerator SelectOption(int index)
        {
            while (true)
            {
                int cursor = selectPlateCtrl.instance.cursor_no;

                if (cursor < index)
                {
                    yield return KeyPresser.PressAndWait(KeyType.Down, Waiter.ShortWait);
                }
                else if (cursor > index)
                {
                    yield return KeyPresser.PressAndWait(KeyType.Up, Waiter.ShortWait);
                }
                else
                {
                    break;
                }
            }

            yield return Waiter.Wait(Waiter.ShortWait);
            KeyPresser.PressKey(KeyType.A);
        }
    }
}
