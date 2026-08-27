using UnityEngine;
using Worldforge.Item;

namespace Worldforge.Inventory
{
    /// <summary>
    /// Event fired when an item quantity is added to a specific inventory container slot.
    /// </summary>
    public readonly struct InventoryItemAddedEvent
    {
        public string ContainerId { get; }
        public ItemDefinition Item { get; }
        public int QuantityAdded { get; }
        public int SlotIndex { get; }
        public float Timestamp { get; }

        public InventoryItemAddedEvent(
            string containerId,
            ItemDefinition item,
            int quantityAdded,
            int slotIndex,
            float timestamp)
        {
            ContainerId = containerId;
            Item = item;
            QuantityAdded = quantityAdded;
            SlotIndex = slotIndex;
            Timestamp = timestamp;
        }
    }

    /// <summary>
    /// Event fired when an item quantity is removed from a specific inventory container slot.
    /// </summary>
    public readonly struct InventoryItemRemovedEvent
    {
        public string ContainerId { get; }
        public ItemDefinition Item { get; }
        public int QuantityRemoved { get; }
        public int SlotIndex { get; }
        public float Timestamp { get; }

        public InventoryItemRemovedEvent(
            string containerId,
            ItemDefinition item,
            int quantityRemoved,
            int slotIndex,
            float timestamp)
        {
            ContainerId = containerId;
            Item = item;
            QuantityRemoved = quantityRemoved;
            SlotIndex = slotIndex;
            Timestamp = timestamp;
        }
    }

    /// <summary>
    /// Event fired when an individual inventory slot's stack state is modified or updated.
    /// </summary>
    public readonly struct InventorySlotChangedEvent
    {
        public string ContainerId { get; }
        public int SlotIndex { get; }
        public ItemStack Stack { get; }
        public float Timestamp { get; }

        public InventorySlotChangedEvent(
            string containerId,
            int slotIndex,
            ItemStack stack,
            float timestamp)
        {
            ContainerId = containerId;
            SlotIndex = slotIndex;
            Stack = stack;
            Timestamp = timestamp;
        }
    }

    /// <summary>
    /// Event fired when the overall contents or weight of an inventory container changes.
    /// </summary>
    public readonly struct InventoryChangedEvent
    {
        public string ContainerId { get; }
        public float CurrentWeight { get; }
        public float MaxWeight { get; }
        public int TotalItemCount { get; }
        public float Timestamp { get; }

        public InventoryChangedEvent(
            string containerId,
            float currentWeight,
            float maxWeight,
            int totalItemCount,
            float timestamp)
        {
            ContainerId = containerId;
            CurrentWeight = currentWeight;
            MaxWeight = maxWeight;
            TotalItemCount = totalItemCount;
            Timestamp = timestamp;
        }
    }

    /// <summary>
    /// Event fired when an inventory container transitions between normal and overencumbered states.
    /// </summary>
    public readonly struct InventoryEncumbranceChangedEvent
    {
        public string ContainerId { get; }
        public bool IsOverencumbered { get; }
        public float CurrentWeight { get; }
        public float MaxWeight { get; }
        public float Timestamp { get; }

        public InventoryEncumbranceChangedEvent(
            string containerId,
            bool isOverencumbered,
            float currentWeight,
            float maxWeight,
            float timestamp)
        {
            ContainerId = containerId;
            IsOverencumbered = isOverencumbered;
            CurrentWeight = currentWeight;
            MaxWeight = maxWeight;
            Timestamp = timestamp;
        }
    }
}
