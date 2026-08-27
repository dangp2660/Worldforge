using System.Collections.Generic;
using Worldforge.Item;

namespace Worldforge.Inventory
{
    /// <summary>
    /// Contract defining operations for an inventory storage container or grid.
    /// </summary>
    public interface IInventoryContainer
    {
        string ContainerId { get; }
        int SlotCount { get; }
        float CurrentWeight { get; }
        float MaxWeight { get; }
        bool IsOverencumbered { get; }

        bool CanAcceptItem(ItemDefinition item, int amount);
        int AddItem(ItemDefinition item, int amount);
        bool RemoveItem(ItemDefinition item, int amount);
        int GetItemCount(ItemDefinition item);
        IReadOnlyList<ItemStack> GetSlots();
        ItemStack GetSlot(int index);
    }
}
