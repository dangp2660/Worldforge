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

        [TestMethod(DisplayName = "Can Accept Item", IsPrimary = false)]
        bool CanAcceptItem(ItemDefinition item, int amount);

        [TestMethod(DisplayName = "Can Accept Stack", IsPrimary = false)]
        bool CanAcceptStack(ItemStack stack);

        [TestMethod(DisplayName = "Get Item Count By Definition", IsPrimary = false)]
        int GetItemCount(ItemDefinition item);

        [TestMethod(DisplayName = "Get Item Count By Code", IsPrimary = true, Order = 4, Description = "Gets total count of item by code in container")]
        int GetItemCount(string itemCode);

        [TestMethod(DisplayName = "Contains Item", IsPrimary = false)]
        bool ContainsItem(ItemDefinition item, int amount = 1);

        [TestMethod(DisplayName = "Contains Item Code", IsPrimary = false)]
        bool ContainsItem(string itemCode, int amount = 1);

        [TestMethod(DisplayName = "Get All Slots", IsPrimary = true, Order = 3, Description = "Returns read-only list of all inventory slots")]
        IReadOnlyList<ItemStack> GetSlots();

        [TestMethod(DisplayName = "Get Slot", IsPrimary = false)]
        ItemStack GetSlot(int index);

        [TestMethod(DisplayName = "Find First Slot With Item", IsPrimary = false)]
        int FindFirstSlotWithItem(ItemDefinition item);

        [TestMethod(DisplayName = "Find First Empty Slot", IsPrimary = false)]
        int FindFirstEmptySlot();

        [TestMethod(DisplayName = "Add Item (Definition)", IsPrimary = true, Order = 1, Description = "Adds items by Definition and amount to container")]
        int AddItem(ItemDefinition item, int amount);

        [TestMethod(DisplayName = "Add Item (Stack)", IsPrimary = false)]
        int AddItem(ItemStack stack);

        [TestMethod(DisplayName = "Add Item To Slot", IsPrimary = false)]
        int AddItemToSlot(int slotIndex, ItemDefinition item, int amount);

        [TestMethod(DisplayName = "Remove Item (Definition)", IsPrimary = true, Order = 2, Description = "Removes specified amount of item from container")]
        bool RemoveItem(ItemDefinition item, int amount);

        [TestMethod(DisplayName = "Remove Item (Code)", IsPrimary = false)]
        bool RemoveItem(string itemCode, int amount);

        [TestMethod(DisplayName = "Remove Item At Slot", IsPrimary = false)]
        bool RemoveItemAt(int slotIndex, int amount, out ItemStack removedStack);

        [TestMethod(DisplayName = "Swap Slots", IsPrimary = false)]
        bool SwapSlots(int sourceIndex, int targetIndex);

        [TestMethod(DisplayName = "Move Item", IsPrimary = false)]
        bool MoveItem(int sourceIndex, int targetIndex);

        [TestMethod(DisplayName = "Split Stack", IsPrimary = false)]
        bool SplitStack(int sourceIndex, int targetIndex, int splitAmount);

        [TestMethod(DisplayName = "Merge Stacks", IsPrimary = false)]
        bool MergeStacks(int sourceIndex, int targetIndex);

        [TestMethod(DisplayName = "Clear Slot", IsPrimary = false)]
        void ClearSlot(int slotIndex);

        [TestMethod(DisplayName = "Clear All Items", IsPrimary = true, Order = 5, Description = "Clears all slots in container")]
        void Clear();

        [TestMethod(DisplayName = "Resize Container", IsPrimary = false)]
        void Resize(int newSlotCount);

        [TestMethod(DisplayName = "Auto Sort", IsPrimary = false)]
        void AutoSort();
    }
}
