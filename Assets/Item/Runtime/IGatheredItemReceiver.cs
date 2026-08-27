namespace Worldforge.Item
{
    /// <summary>
    /// Contract for entities capable of receiving items gathered from resource nodes.
    /// </summary>
    public interface IGatheredItemReceiver
    {
        /// <summary>
        /// Attempts to receive the specified item and amount.
        /// </summary>
        /// <param name="item">The item definition being transferred.</param>
        /// <param name="amount">The quantity of the item.</param>
        /// <returns>True if the item was successfully accepted, otherwise false.</returns>
        bool ReceiveItem(ItemDefinition item, int amount);
    }
}
