using System;
using System.Collections.Generic;
using Worldforge.Item;

namespace Worldforge.Inventory
{
    /// <summary>
    /// Contract defining operations for an inventory storage container or grid.
    /// Manages slot access, item queries, additions/removals, stacking, and event subscriptions.
    /// </summary>
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
        int GetItemCount(string itemCode);
        bool ContainsItem(ItemDefinition item, int amount = 1);
        bool ContainsItem(string itemCode, int amount = 1);
        IReadOnlyList<ItemStack> GetSlots();
        ItemStack GetSlot(int index);
        int FindFirstSlotWithItem(ItemDefinition item);
        int FindFirstEmptySlot();

        int AddItem(ItemDefinition item, int amount);
        int AddItem(ItemStack stack);
        int AddItemToSlot(int slotIndex, ItemDefinition item, int amount);
        bool RemoveItem(ItemDefinition item, int amount);
        bool RemoveItem(string itemCode, int amount);
        bool RemoveItemAt(int slotIndex, int amount, out ItemStack removedStack);
        bool SwapSlots(int sourceIndex, int targetIndex);
        bool MoveItem(int sourceIndex, int targetIndex);
        bool SplitStack(int sourceIndex, int targetIndex, int splitAmount);
        bool MergeStacks(int sourceIndex, int targetIndex);
        void ClearSlot(int slotIndex);
        void Clear();
        void Resize(int newSlotCount);
        void AutoSort();
    }
}
