using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using Newtonsoft.Json.Schema;
using System.Linq;
using System.Runtime.Serialization;

namespace TurnaboutAI.NeuroAPI
{
    public class ClientMessage
    {
        [JsonProperty("command")]
        public string Command { get; set; }

        [JsonProperty("game")]
        public string Game { get; set; }

        public override string ToString()
        {
            return Command;
        }
    }

    public sealed class ClientDataMessage<T> : ClientMessage
    {
        [JsonProperty("data")]
        public T Data { get; set; }

        public override string ToString()
        {
            return $"{Command} - {Data}";
        }
    }

    public sealed class ClientActionResultData
    {
        [JsonProperty("id")]
        public string Id { get; set; }

        [JsonProperty("success")]
        public bool Success { get; set; }

        [JsonProperty("message", NullValueHandling = NullValueHandling.Ignore)]
        public string Message { get; set; }

        public override string ToString()
        {
            return $"Id:{Id} Succes:{Success} Message:{Message}";
        }
    }

    public sealed class Action
    {
        [JsonProperty("name")]
        public string Name { get; set; }

        [JsonProperty("description")]
        public string Description { get; set; }

        [JsonProperty("schema")]
        public JsonSchema Schema { get; set; }

        public override string ToString()
        {
            return Name;
        }
    }

    public sealed class RegisterActionsData
    {
        [JsonProperty("actions")]
        public Action[] Actions { get; set; }

        public override string ToString()
        {
            if (Actions == null) return "[]";

            return $"[\"{string.Join("\",\"", Actions.Select(a => a.ToString()).ToArray())}\"]";
        }
    }

    public sealed class UnregisterActionsData
    {
        [JsonProperty("action_names")]
        public string[] ActionNames { get; set; }

        public override string ToString()
        {
            if(ActionNames == null) return "[]";

            return $"[\"{string.Join("\",\"", ActionNames)}\"]";
        }
    }

    public enum ForcePriority
    {
        [EnumMember(Value = "low")]
        Low,

        [EnumMember(Value = "medium")]
        Medium,

        [EnumMember(Value = "high")]
        High,

        [EnumMember(Value = "critical")]
        Critical
    }

    public sealed class ForceActionsData
    {
        [JsonProperty("state")]
        public string State { get; set; }

        [JsonProperty("query")]
        public string Query { get; set; }

        [JsonProperty("ephemeral_context")]
        public bool EphemeralContext { get; set; }

        [JsonProperty("priority"), JsonConverter(typeof(StringEnumConverter))]
        public ForcePriority Priority { get; set; }

        [JsonProperty("actions_names")]
        public string[] ActionNames { get; set; }
    }

    public sealed class ClientContextData
    {
        [JsonProperty("message")]
        public string Message { get; set; }

        [JsonProperty("silent")]
        public bool Silent { get; set; }

        public override string ToString()
        {
            return $"Message: \"{Message}\" Silent: {Silent}";
        }
    }
}
