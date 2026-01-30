using TurnaboutAI.NeuroAPI;
using TurnaboutAI.Patches;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Newtonsoft.Json.Schema;
using System.Linq;

namespace TurnaboutAI.Actions
{
    /// <summary>
    /// Gives Neuro a list of all the psychological profiles.
    /// </summary>
    internal sealed class ReviewProfilesAction : IActionHandler
    {
        public CancellationToken CancellationToken { get; set; }

        public string Name => "review_profiles";

        public string Description => "Gets all the psychological profiles of individuals involved in the investigation.";

        public JsonSchema Schema => null;

        public void Execute(JObject data)
        {
            if (!CancellationToken.IsCancellationRequested)
            {
                if (!RecordPatch.GetProfiles().Any())
                {
                    return;
                }

                var items = RecordPatch.GetProfiles().Select(e => new
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
