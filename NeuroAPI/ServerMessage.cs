using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace TurnaboutAI.NeuroAPI
{
    public sealed class ServerMessage
    {
        [JsonProperty("command")]
        public string Command { get; set; }

        [JsonProperty("data")]
        public JObject Data { get; set; }
    }

    public sealed class ServerActionData
    {
        [JsonProperty("id")]
        public string Id { get; set; }

        [JsonProperty("name")]
        public string Name { get; set; }

        [JsonProperty("data")]
        public string Data { get; set; }
    }

    public sealed class ServerShutdownData
    {
        [JsonProperty("wants_shutdown")]
        public bool WantsShutdown { get; set; }
    }
}
