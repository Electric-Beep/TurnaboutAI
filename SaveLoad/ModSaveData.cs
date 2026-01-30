namespace TurnaboutAI.SaveLoad
{
    /// <summary>
    /// Encapsultes data that can be used to save the game at a given point.
    /// </summary>
    public sealed class ModSaveData
    {
        public byte[] SaveDataBytes { get; set; }
        public SaveData SaveData { get; set; }
    }
}
