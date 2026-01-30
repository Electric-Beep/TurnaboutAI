using TurnaboutAI.NeuroAPI;
using TurnaboutAI.Patches;
using Newtonsoft.Json.Linq;
using Newtonsoft.Json.Schema;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using TurnaboutAI.Utility;

namespace TurnaboutAI.Actions
{
    /// <summary>
    /// Selects an option during multiple choice sections.
    /// </summary>
    public sealed class QuestionAction : IActionHandler
    {
        private readonly List<TalkOption> _options;

        public QuestionAction(List<TalkOption> options)
        {
            _options = options;
        }

        public CancellationToken CancellationToken { get; set; }

        public string Name => "ask_question";

        public string Description => "Ask a question.";

        public JsonSchema Schema => new JsonSchema
        {
            Type = JsonSchemaType.Object,
            Properties = new Dictionary<string, JsonSchema>
            {
                { "question", Utilities.JEnum(_options.Where(o => !o.Read).Select(o => o.Text)) }
            }
        };

        public void Execute(JObject data)
        {
            if(!CancellationToken.IsCancellationRequested)
            {
                ModStateManager.Instance.UnregisterAllActions();

                string question = data?["question"]?.Value<string>();

                var option = _options.FirstOrDefault(o => o.Text == question);

                if(option != null)
                {
                    int index = _options.IndexOf(option);

                    coroutineCtrl.instance.Play(new SequenceEnumerator(SelectOption(index)));
                }
            }
        }

        public bool Validate(JObject data, out string message)
        {
            if(CancellationToken.IsCancellationRequested)
            {
                message = null;
                return true;
            }

            string question = data?["question"]?.Value<string>();

            if (string.IsNullOrEmpty(question) || !_options.Any(o => o.Text == question))
            {
                message = "Please choose a valid question.";
                return false;
            }

            message = string.Empty;
            return true;
        }

        private IEnumerator SelectOption(int index)
        {
            while(true)
            {
                int cursor = selectPlateCtrl.instance.cursor_no;

                if(cursor < index)
                {
                    yield return KeyPresser.PressAndWait(KeyType.Down, Waiter.ShortWait);
                }
                else if(cursor > index)
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
