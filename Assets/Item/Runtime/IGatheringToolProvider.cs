namespace Worldforge.Item
{
    /// <summary>
    /// Contract for entities that can provide an active gathering tool (e.g. Character, Equipment Loadout, Inventory).
    /// </summary>
    public interface IGatheringToolProvider
    {
        /// <summary>
        /// Gets the currently active gathering tool, or null if none is equipped.
        /// </summary>
        IGatheringTool ActiveTool { get; }
    }
}
