using TurnaboutAI.Utility;

namespace TurnaboutAI.Actions
{
    /// <summary>
    /// Represents a spot that can be examined during Inspect sections.
    /// </summary>
    public sealed class ExamineSpot
    {
        /// <summary>
        /// Spot name.
        /// </summary>
        /// <remarks>
        /// The game did not make it easy to label them...
        /// It could be done, but would take a ton of manual work.
        /// </remarks>
        public string Name { get; set; }

        /// <summary>
        /// Spot geometry.
        /// </summary>
        public Polygon Polygon { get; set; }
    }
}
