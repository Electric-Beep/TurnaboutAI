using Newtonsoft.Json.Linq;
using Newtonsoft.Json.Schema;

namespace TurnaboutAI.NeuroAPI
{
    /// <summary>
    /// Represents an action Neuro can perform.
    /// </summary>
    public interface IActionHandler
    {
        /// <summary>
        /// A token to check if the handler should not execute.
        /// </summary>
        CancellationToken CancellationToken { get; set; }

        /// <summary>
        /// Action name.
        /// </summary>
        string Name { get; }

        /// <summary>
        /// Action description.
        /// </summary>
        string Description { get; }

        /// <summary>
        /// Action schema.
        /// </summary>
        JsonSchema Schema { get; }

        /// <summary>
        /// Validates that the data object contains valid data.
        /// </summary>
        /// <param name="data">Data returned by Neuro.</param>
        /// <param name="message">Error message if validation fails.</param>
        /// <returns>True if data is valid, otherwise false.</returns>
        bool Validate(JObject data, out string message);

        /// <summary>
        /// Executes the handler.
        /// </summary>
        /// <param name="data">Data passed by Neuro.</param>
        void Execute(JObject data);
    }
}
