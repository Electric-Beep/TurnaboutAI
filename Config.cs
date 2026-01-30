namespace TurnaboutAI
{
    /// <summary>
    /// Encapsulates mod configuration values.
    /// </summary>
    public sealed class Config
    {
        /// <summary>
        /// If true, a console window will be opened.
        /// </summary>
        /// <remarks>Default value: true</remarks>
        public bool LogToConsole { get; set; } = true;

        /// <summary>
        /// If true, no actions will be sent to Neuro, only dialogue.
        /// </summary>
        /// <remarks>Default value: false</remarks>
        public bool OnlyText { get; set; }

        /// <summary>
        /// If true, the mod will automatically save the game at certain points
        /// where it is possible to game over.
        /// </summary>
        /// <remarks>Default value: true</remarks>
        public bool SafetySave { get; set; } = true;

        /// <summary>
        /// Sets which save slot to use for safety saves.
        /// </summary>
        /// <remarks>The first slot is slot 0. There are 10 slots.</remarks>
        public int SaveSlot { get; set; }

        /// <summary>
        /// Url of the game server.
        /// </summary>
        /// <remarks>If set, the mod will use this URL over one
        /// set in an environment variable.</remarks>
        public string WebSocketUrl { get; set; }
    }
}
