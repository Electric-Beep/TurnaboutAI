using TurnaboutAI.NeuroAPI;
using TurnaboutAI.Patches;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Newtonsoft.Json.Schema;
using System.Linq;

namespace TurnaboutAI.Actions
{
    /// <summary>
    /// Gives Neuro a list of all the evidence.
    /// </summary>
    internal sealed class ReviewEvidenceAction : IActionHandler
    {
        public CancellationToken CancellationToken { get; set; }

        public string Name => "review_evidence";

        public string Description => "Gets all evidence you've collected so far.";

        public JsonSchema Schema => null;

        public void Execute(JObject data)
        {
            if(!CancellationToken.IsCancellationRequested)
            {
                if(!RecordPatch.GetEvidence().Any())
                {
                    return;
                }

                var items = RecordPatch.GetEvidence().Select(e => new
                {
                    e.Name,
                    e.Description,
                });

                string json = JsonConvert.SerializeObject(items);
                ModStateManager.Instance.SendContext(json, true);
            }
        }

        public bool Validate(JObject data, out string message)
        {
            message = null;
            return true;
        }
    }
}
