using UnityEngine;

namespace Worldforge.Item
{
    /// <summary>
    /// Contract representing an item residing inside an inventory slot or container.
    /// </summary>
    public interface IInventoryItem
    {
        ItemDefinition Definition { get; }
        int Quantity { get; }
        float TotalWeight { get; }
        Vector2Int GridSize { get; }
        bool IsRotated { get; }
    }
}
