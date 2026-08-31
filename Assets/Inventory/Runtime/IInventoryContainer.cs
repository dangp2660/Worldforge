using System;
using System.Collections.Generic;
using Worldforge.Core.Attributes;
using Worldforge.Item;

namespace Worldforge.Inventory
{
    /// <summary>
    /// Contract defining operations for an inventory storage container or grid.
    /// Manages slot access, item queries, additions/removals, stacking, and event subscriptions.
    /// </summary>
    [TestTarget(Category = "Inventory", DisplayName = "Inventory Container", Order = 20)]
    public interface IInventoryContainer
    {
        string ContainerId { get; }
        int SlotCount { get; }
        float CurrentWeight { get; }
        float MaxWeight { get; set; }
        bool IsOverencumbered { get; }
        int TotalItemCount { get; }
        int EmptySlotCount { get; }

        event Action<InventoryItemAddedEvent> ItemAdded;
        event Action<InventoryItemRemovedEvent> ItemRemoved;
        event Action<InventorySlotChangedEvent> SlotChanged;
        event Action<InventoryChangedEvent> InventoryChanged;
        event Action<InventoryEncumbranceChangedEvent> EncumbranceChanged;

        bool CanAcceptItem(ItemDefinition item, int amount);

        bool CanAcceptStack(ItemStack stack);

        int GetItemCount(ItemDefinition item);

        [TestMethod(DisplayName = "Get Item Count By Code", Order = 4, Description = "Gets total count of item by code in container")]
        int GetItemCount(string itemCode);

        bool ContainsItem(ItemDefinition item, int amount = 1);

        bool ContainsItem(string itemCode, int amount = 1);

        [TestMethod(DisplayName = "Get All Slots", Order = 3, Description = "Returns read-only list of all inventory slots")]
        IReadOnlyList<ItemStack> GetSlots();

        ItemStack GetSlot(int index);

        int FindFirstSlotWithItem(ItemDefinition item);

        int FindFirstEmptySlot();

        [TestMethod(DisplayName = "Add Item (Definition)", Order = 1, Description = "Adds items by Definition and amount to container")]
        int AddItem(ItemDefinition item, int amount);

        int AddItem(ItemStack stack);

        int AddItemToSlot(int slotIndex, ItemDefinition item, int amount);

        [TestMethod(DisplayName = "Remove Item (Definition)", Order = 2, Description = "Removes specified amount of item from container")]
        bool RemoveItem(ItemDefinition item, int amount);

        bool RemoveItem(string itemCode, int amount);

        bool RemoveItemAt(int slotIndex, int amount, out ItemStack removedStack);

        bool SwapSlots(int sourceIndex, int targetIndex);

        bool MoveItem(int sourceIndex, int targetIndex);

        bool SplitStack(int sourceIndex, int targetIndex, int splitAmount);

        bool MergeStacks(int sourceIndex, int targetIndex);

        void ClearSlot(int slotIndex);

        [TestMethod(DisplayName = "Clear All Items", Order = 5, Description = "Clears all slots in container")]
        void Clear();

        void Resize(int newSlotCount);

        void AutoSort();
    }
}
