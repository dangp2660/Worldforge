using System;
using System.Collections.Generic;
using UnityEngine;
using Worldforge.Item;

namespace Worldforge.Inventory
{
    /// <summary>
    /// Core container implementation managing item slots, stacking logic, weight tracking,
    /// encumbrance calculation, auto-sorting, and event publishing.
    /// Implements <see cref="IInventoryContainer"/> and <see cref="IGatheredItemReceiver"/>.
    /// </summary>
    [Serializable]
    public sealed class InventoryContainer : IInventoryContainer, IGatheredItemReceiver
    {
        [SerializeField] private string _containerId = "PlayerInventory";
        [SerializeField] private int _slotCount = 20;
        [SerializeField] private float _maxWeight = 50f;
        [SerializeField] private List<ItemStack> _slots = new();

        private bool _lastEncumberedState;

        public event Action<InventoryItemAddedEvent> ItemAdded;
        public event Action<InventoryItemRemovedEvent> ItemRemoved;
        public event Action<InventorySlotChangedEvent> SlotChanged;
        public event Action<InventoryChangedEvent> InventoryChanged;
        public event Action<InventoryEncumbranceChangedEvent> EncumbranceChanged;

        // Legacy event for backward compatibility
        public event Action OnInventoryChanged;

        public InventoryContainer()
        {
            InitializeSlots(_slotCount);
            _lastEncumberedState = IsOverencumbered;
        }

        public InventoryContainer(string containerId, int slotCount = 20, float maxWeight = 50f)
        {
            _containerId = string.IsNullOrWhiteSpace(containerId) ? Guid.NewGuid().ToString() : containerId;
            _slotCount = Mathf.Max(1, slotCount);
            _maxWeight = Mathf.Max(1f, maxWeight);
            InitializeSlots(_slotCount);
            _lastEncumberedState = IsOverencumbered;
        }

        public InventoryContainer(InventoryDefinition definition, string containerId = null)
        {
            if (definition != null)
            {
                _containerId = string.IsNullOrWhiteSpace(containerId) ? definition.InventoryCode : containerId;
                _slotCount = Mathf.Max(1, definition.SlotCount);
                _maxWeight = Mathf.Max(1f, definition.WeightLimit);
                InitializeSlots(_slotCount);

                if (definition.StartingItems != null && definition.StartingItems.Count > 0)
                {
                    for (var i = 0; i < definition.StartingItems.Count; i++)
                    {
                        var entry = definition.StartingItems[i];
                        if (entry.Item != null && entry.Amount > 0)
                        {
                            AddItem(entry.Item, entry.Amount);
                        }
                    }
                }
            }
            else
            {
                _containerId = string.IsNullOrWhiteSpace(containerId) ? Guid.NewGuid().ToString() : containerId;
                _slotCount = 20;
                _maxWeight = 50f;
                InitializeSlots(_slotCount);
            }

            _lastEncumberedState = IsOverencumbered;
        }

        public string ContainerId
        {
            get { return _containerId; }
        }

        public int SlotCount
        {
            get { return _slotCount; }
        }

        public float CurrentWeight
        {
            get
            {
                var total = 0f;
                for (var i = 0; i < _slots.Count; i++)
                {
                    var slot = _slots[i];
                    if (slot != null && !slot.IsEmpty)
                    {
                        total += slot.TotalWeight;
                    }
                }
                return total;
            }
        }

        public float MaxWeight
        {
            get { return _maxWeight; }
            set
            {
                _maxWeight = Mathf.Max(1f, value);
                CheckEncumbranceChange();
                PublishInventoryChanged();
            }
        }

        public bool IsOverencumbered
        {
            get { return CurrentWeight > _maxWeight; }
        }

        public int TotalItemCount
        {
            get
            {
                var count = 0;
                for (var i = 0; i < _slots.Count; i++)
                {
                    var slot = _slots[i];
                    if (slot != null && !slot.IsEmpty)
                    {
                        count += slot.Quantity;
                    }
                }
                return count;
            }
        }

        public int EmptySlotCount
        {
            get
            {
                var count = 0;
                for (var i = 0; i < _slots.Count; i++)
                {
                    var slot = _slots[i];
                    if (slot == null || slot.IsEmpty)
                    {
                        count++;
                    }
                }
                return count;
            }
        }

        public bool CanAcceptItem(ItemDefinition item, int amount)
        {
            if (item == null || amount <= 0)
            {
                return false;
            }

            var remaining = amount;

            if (item.IsStackable)
            {
                for (var i = 0; i < _slots.Count; i++)
                {
                    var slot = _slots[i];
                    if (slot != null && slot.CanStackWith(item))
                    {
                        remaining -= slot.AvailableSpace;
                        if (remaining <= 0)
                        {
                            return true;
                        }
                    }
                }
            }

            var itemsPerEmptySlot = item.IsStackable ? item.MaxStack : 1;
            for (var i = 0; i < _slots.Count; i++)
            {
                var slot = _slots[i];
                if (slot == null || slot.IsEmpty)
                {
                    remaining -= itemsPerEmptySlot;
                    if (remaining <= 0)
                    {
                        return true;
                    }
                }
            }

            return remaining <= 0;
        }

        public bool CanAcceptStack(ItemStack stack)
        {
            if (stack == null || stack.IsEmpty)
            {
                return true;
            }

            return CanAcceptItem(stack.Item, stack.Quantity);
        }

        public int GetItemCount(ItemDefinition item)
        {
            if (item == null)
            {
                return 0;
            }

            var count = 0;
            for (var i = 0; i < _slots.Count; i++)
            {
                var slot = _slots[i];
                if (slot != null && !slot.IsEmpty && slot.Item != null)
                {
                    if (slot.Item == item || string.Equals(slot.Item.ItemCode, item.ItemCode, StringComparison.Ordinal))
                    {
                        count += slot.Quantity;
                    }
                }
            }

            return count;
        }

        public int GetItemCount(string itemCode)
        {
            if (string.IsNullOrWhiteSpace(itemCode))
            {
                return 0;
            }

            var count = 0;
            for (var i = 0; i < _slots.Count; i++)
            {
                var slot = _slots[i];
                if (slot != null && !slot.IsEmpty && slot.Item != null)
                {
                    if (string.Equals(slot.Item.ItemCode, itemCode, StringComparison.Ordinal))
                    {
                        count += slot.Quantity;
                    }
                }
            }

            return count;
        }

        public bool ContainsItem(ItemDefinition item, int amount = 1)
        {
            if (item == null || amount <= 0)
            {
                return false;
            }

            return GetItemCount(item) >= amount;
        }

        public bool ContainsItem(string itemCode, int amount = 1)
        {
            if (string.IsNullOrWhiteSpace(itemCode) || amount <= 0)
            {
                return false;
            }

            return GetItemCount(itemCode) >= amount;
        }

        public IReadOnlyList<ItemStack> GetSlots()
        {
            return _slots;
        }

        public ItemStack GetSlot(int index)
        {
            if (index < 0 || index >= _slots.Count)
            {
                return null;
            }

            return _slots[index];
        }

        public int FindFirstSlotWithItem(ItemDefinition item)
        {
            if (item == null)
            {
                return -1;
            }

            for (var i = 0; i < _slots.Count; i++)
            {
                var slot = _slots[i];
                if (slot != null && !slot.IsEmpty && slot.Item != null)
                {
                    if (slot.Item == item || string.Equals(slot.Item.ItemCode, item.ItemCode, StringComparison.Ordinal))
                    {
                        return i;
                    }
                }
            }

            return -1;
        }

        public int FindFirstEmptySlot()
        {
            for (var i = 0; i < _slots.Count; i++)
            {
                var slot = _slots[i];
                if (slot == null || slot.IsEmpty)
                {
                    return i;
                }
            }

            return -1;
        }

        public int AddItem(ItemDefinition item, int amount)
        {
            if (item == null || amount <= 0)
            {
                return 0;
            }

            var remaining = amount;
            var totalAdded = 0;
            var time = Time.time;

            // Step 1: Top-up existing non-full stacks if item is stackable
            if (item.IsStackable)
            {
                for (var i = 0; i < _slots.Count; i++)
                {
                    if (remaining <= 0)
                    {
                        break;
                    }

                    var slot = _slots[i];
                    if (slot != null && slot.CanStackWith(item))
                    {
                        var beforeQty = slot.Quantity;
                        var added = slot.Add(remaining, out remaining);
                        if (added > 0)
                        {
                            totalAdded += added;
                            SlotChanged?.Invoke(new InventorySlotChangedEvent(_containerId, i, slot, time));
                            ItemAdded?.Invoke(new InventoryItemAddedEvent(_containerId, item, added, i, time));
                        }
                    }
                }
            }

            // Step 2: Fill empty slots
            if (remaining > 0)
            {
                for (var i = 0; i < _slots.Count; i++)
                {
                    if (remaining <= 0)
                    {
                        break;
                    }

                    var slot = _slots[i];
                    if (slot == null || slot.IsEmpty)
                    {
                        var placeAmount = item.IsStackable ? Mathf.Min(remaining, item.MaxStack) : 1;
                        _slots[i] = new ItemStack(item, placeAmount);
                        remaining -= placeAmount;
                        totalAdded += placeAmount;

                        SlotChanged?.Invoke(new InventorySlotChangedEvent(_containerId, i, _slots[i], time));
                        ItemAdded?.Invoke(new InventoryItemAddedEvent(_containerId, item, placeAmount, i, time));
                    }
                }
            }

            if (totalAdded > 0)
            {
                CheckEncumbranceChange();
                PublishInventoryChanged();
            }

            return totalAdded;
        }

        public int AddItem(ItemStack stack)
        {
            if (stack == null || stack.IsEmpty)
            {
                return 0;
            }

            var item = stack.Item;
            if (item == null)
            {
                return 0;
            }

            if (item.IsStackable)
            {
                var added = AddItem(item, stack.Quantity);
                stack.Quantity -= added;
                return added;
            }

            // Non-stackable: find empty slot and transfer instance directly
            var emptyIndex = FindFirstEmptySlot();
            if (emptyIndex >= 0)
            {
                _slots[emptyIndex] = new ItemStack(
                    item,
                    1,
                    stack.CurrentDurability,
                    stack.CustomName)
                {
                    IsLocked = stack.IsLocked,
                    IsRotated = stack.IsRotated
                };

                stack.Clear();
                var time = Time.time;
                SlotChanged?.Invoke(new InventorySlotChangedEvent(_containerId, emptyIndex, _slots[emptyIndex], time));
                ItemAdded?.Invoke(new InventoryItemAddedEvent(_containerId, item, 1, emptyIndex, time));

                CheckEncumbranceChange();
                PublishInventoryChanged();
                return 1;
            }

            return 0;
        }

        public int AddItemToSlot(int slotIndex, ItemDefinition item, int amount)
        {
            if (slotIndex < 0 || slotIndex >= _slots.Count || item == null || amount <= 0)
            {
                return 0;
            }

            var slot = _slots[slotIndex];
            var time = Time.time;

            if (slot == null || slot.IsEmpty)
            {
                var placeAmount = item.IsStackable ? Mathf.Min(amount, item.MaxStack) : 1;
                _slots[slotIndex] = new ItemStack(item, placeAmount);

                SlotChanged?.Invoke(new InventorySlotChangedEvent(_containerId, slotIndex, _slots[slotIndex], time));
                ItemAdded?.Invoke(new InventoryItemAddedEvent(_containerId, item, placeAmount, slotIndex, time));

                CheckEncumbranceChange();
                PublishInventoryChanged();
                return placeAmount;
            }

            if (item.IsStackable && slot.CanStackWith(item))
            {
                var added = slot.Add(amount, out _);
                if (added > 0)
                {
                    SlotChanged?.Invoke(new InventorySlotChangedEvent(_containerId, slotIndex, slot, time));
                    ItemAdded?.Invoke(new InventoryItemAddedEvent(_containerId, item, added, slotIndex, time));

                    CheckEncumbranceChange();
                    PublishInventoryChanged();
                }
                return added;
            }

            return 0;
        }

        public bool RemoveItem(ItemDefinition item, int amount)
        {
            if (item == null || amount <= 0)
            {
                return false;
            }

            if (GetItemCount(item) < amount)
            {
                return false;
            }

            var remaining = amount;
            var time = Time.time;

            for (var i = _slots.Count - 1; i >= 0; i--)
            {
                if (remaining <= 0)
                {
                    break;
                }

                var slot = _slots[i];
                if (slot != null && !slot.IsEmpty && slot.Item != null)
                {
                    if (slot.Item == item || string.Equals(slot.Item.ItemCode, item.ItemCode, StringComparison.Ordinal))
                    {
                        var removed = slot.Remove(remaining, out _);
                        remaining -= removed;

                        SlotChanged?.Invoke(new InventorySlotChangedEvent(_containerId, i, slot, time));
                        ItemRemoved?.Invoke(new InventoryItemRemovedEvent(_containerId, item, removed, i, time));
                    }
                }
            }

            CheckEncumbranceChange();
            PublishInventoryChanged();
            return true;
        }

        public bool RemoveItem(string itemCode, int amount)
        {
            if (string.IsNullOrWhiteSpace(itemCode) || amount <= 0)
            {
                return false;
            }

            if (GetItemCount(itemCode) < amount)
            {
                return false;
            }

            var remaining = amount;
            var time = Time.time;
            ItemDefinition removedItemDef = null;

            for (var i = _slots.Count - 1; i >= 0; i--)
            {
                if (remaining <= 0)
                {
                    break;
                }

                var slot = _slots[i];
                if (slot != null && !slot.IsEmpty && slot.Item != null)
                {
                    if (string.Equals(slot.Item.ItemCode, itemCode, StringComparison.Ordinal))
                    {
                        removedItemDef ??= slot.Item;
                        var removed = slot.Remove(remaining, out _);
                        remaining -= removed;

                        SlotChanged?.Invoke(new InventorySlotChangedEvent(_containerId, i, slot, time));
                        ItemRemoved?.Invoke(new InventoryItemRemovedEvent(_containerId, removedItemDef, removed, i, time));
                    }
                }
            }

            CheckEncumbranceChange();
            PublishInventoryChanged();
            return true;
        }

        public bool RemoveItemAt(int slotIndex, int amount, out ItemStack removedStack)
        {
            removedStack = null;
            if (slotIndex < 0 || slotIndex >= _slots.Count || amount <= 0)
            {
                return false;
            }

            var slot = _slots[slotIndex];
            if (slot == null || slot.IsEmpty || slot.Item == null)
            {
                return false;
            }

            var itemDef = slot.Item;
            var durability = slot.CurrentDurability;
            var customName = slot.CustomName;
            var isLocked = slot.IsLocked;

            var removedQty = slot.Remove(amount, out _);
            if (removedQty <= 0)
            {
                return false;
            }

            removedStack = new ItemStack(itemDef, removedQty, durability, customName)
            {
                IsLocked = isLocked
            };

            var time = Time.time;
            SlotChanged?.Invoke(new InventorySlotChangedEvent(_containerId, slotIndex, slot, time));
            ItemRemoved?.Invoke(new InventoryItemRemovedEvent(_containerId, itemDef, removedQty, slotIndex, time));

            CheckEncumbranceChange();
            PublishInventoryChanged();
            return true;
        }

        public bool SwapSlots(int sourceIndex, int targetIndex)
        {
            if (sourceIndex < 0 || sourceIndex >= _slots.Count || targetIndex < 0 || targetIndex >= _slots.Count)
            {
                return false;
            }

            if (sourceIndex == targetIndex)
            {
                return true;
            }

            var temp = _slots[sourceIndex];
            _slots[sourceIndex] = _slots[targetIndex];
            _slots[targetIndex] = temp;

            var time = Time.time;
            SlotChanged?.Invoke(new InventorySlotChangedEvent(_containerId, sourceIndex, _slots[sourceIndex], time));
            SlotChanged?.Invoke(new InventorySlotChangedEvent(_containerId, targetIndex, _slots[targetIndex], time));

            PublishInventoryChanged();
            return true;
        }

        public bool MoveItem(int sourceIndex, int targetIndex)
        {
            if (sourceIndex < 0 || sourceIndex >= _slots.Count || targetIndex < 0 || targetIndex >= _slots.Count)
            {
                return false;
            }

            if (sourceIndex == targetIndex)
            {
                return true;
            }

            var source = _slots[sourceIndex];
            var target = _slots[targetIndex];

            if (source == null || source.IsEmpty)
            {
                return false;
            }

            // Target is empty: straightforward move
            if (target == null || target.IsEmpty)
            {
                _slots[targetIndex] = source;
                _slots[sourceIndex] = new ItemStack();

                var time = Time.time;
                SlotChanged?.Invoke(new InventorySlotChangedEvent(_containerId, sourceIndex, _slots[sourceIndex], time));
                SlotChanged?.Invoke(new InventorySlotChangedEvent(_containerId, targetIndex, _slots[targetIndex], time));

                PublishInventoryChanged();
                return true;
            }

            // Target has same item and is stackable: merge
            if (target.CanStackWith(source))
            {
                return MergeStacks(sourceIndex, targetIndex);
            }

            // Target has different item: swap
            return SwapSlots(sourceIndex, targetIndex);
        }

        public bool SplitStack(int sourceIndex, int targetIndex, int splitAmount)
        {
            if (sourceIndex < 0 || sourceIndex >= _slots.Count || targetIndex < 0 || targetIndex >= _slots.Count)
            {
                return false;
            }

            if (sourceIndex == targetIndex || splitAmount <= 0)
            {
                return false;
            }

            var source = _slots[sourceIndex];
            var target = _slots[targetIndex];

            if (source == null || source.IsEmpty || source.Quantity <= splitAmount)
            {
                return false;
            }

            if (target != null && !target.IsEmpty)
            {
                return false;
            }

            var itemDef = source.Item;
            source.Remove(splitAmount, out _);
            _slots[targetIndex] = new ItemStack(itemDef, splitAmount, source.CurrentDurability, source.CustomName);

            var time = Time.time;
            SlotChanged?.Invoke(new InventorySlotChangedEvent(_containerId, sourceIndex, _slots[sourceIndex], time));
            SlotChanged?.Invoke(new InventorySlotChangedEvent(_containerId, targetIndex, _slots[targetIndex], time));

            PublishInventoryChanged();
            return true;
        }

        public bool MergeStacks(int sourceIndex, int targetIndex)
        {
            if (sourceIndex < 0 || sourceIndex >= _slots.Count || targetIndex < 0 || targetIndex >= _slots.Count)
            {
                return false;
            }

            if (sourceIndex == targetIndex)
            {
                return false;
            }

            var source = _slots[sourceIndex];
            var target = _slots[targetIndex];

            if (source == null || source.IsEmpty || target == null || target.IsEmpty)
            {
                return false;
            }

            if (!target.CanStackWith(source))
            {
                return false;
            }

            var targetSpace = target.AvailableSpace;
            if (targetSpace <= 0)
            {
                return false;
            }

            var moveAmount = Mathf.Min(targetSpace, source.Quantity);
            target.Add(moveAmount, out _);
            source.Remove(moveAmount, out _);

            var time = Time.time;
            SlotChanged?.Invoke(new InventorySlotChangedEvent(_containerId, sourceIndex, _slots[sourceIndex], time));
            SlotChanged?.Invoke(new InventorySlotChangedEvent(_containerId, targetIndex, _slots[targetIndex], time));

            PublishInventoryChanged();
            return true;
        }

        public void ClearSlot(int slotIndex)
        {
            if (slotIndex < 0 || slotIndex >= _slots.Count)
            {
                return;
            }

            var slot = _slots[slotIndex];
            if (slot != null && !slot.IsEmpty)
            {
                var itemDef = slot.Item;
                var qty = slot.Quantity;
                slot.Clear();

                var time = Time.time;
                SlotChanged?.Invoke(new InventorySlotChangedEvent(_containerId, slotIndex, slot, time));
                ItemRemoved?.Invoke(new InventoryItemRemovedEvent(_containerId, itemDef, qty, slotIndex, time));

                CheckEncumbranceChange();
                PublishInventoryChanged();
            }
        }

        public void Clear()
        {
            for (var i = 0; i < _slots.Count; i++)
            {
                var slot = _slots[i];
                if (slot != null && !slot.IsEmpty)
                {
                    slot.Clear();
                    SlotChanged?.Invoke(new InventorySlotChangedEvent(_containerId, i, slot, Time.time));
                }
            }

            CheckEncumbranceChange();
            PublishInventoryChanged();
        }

        public void Resize(int newSlotCount)
        {
            _slotCount = Mathf.Max(1, newSlotCount);
            while (_slots.Count < _slotCount)
            {
                _slots.Add(new ItemStack());
            }

            PublishInventoryChanged();
        }

        /// <summary>
        /// Sắp xếp và dồn gọn kho đồ theo tiêu chuẩn GDD Section 49:
        /// Gộp các stack cùng loại và sắp xếp theo Category -> Rarity -> Name -> Quantity.
        /// </summary>
        public void AutoSort()
        {
            // Step 1: Consolidate stackables and gather all items
            var stackableGroups = new Dictionary<string, (ItemDefinition item, int quantity)>(StringComparer.Ordinal);
            var nonStackableList = new List<ItemStack>();

            for (var i = 0; i < _slots.Count; i++)
            {
                var slot = _slots[i];
                if (slot == null || slot.IsEmpty || slot.Item == null)
                {
                    continue;
                }

                if (slot.Item.IsStackable)
                {
                    var code = slot.Item.ItemCode;
                    if (stackableGroups.TryGetValue(code, out var existing))
                    {
                        stackableGroups[code] = (existing.item, existing.quantity + slot.Quantity);
                    }
                    else
                    {
                        stackableGroups[code] = (slot.Item, slot.Quantity);
                    }
                }
                else
                {
                    nonStackableList.Add(slot);
                }
            }

            // Step 2: Create compact stacks from consolidated stackables
            var compactStacks = new List<ItemStack>();
            foreach (var kvp in stackableGroups)
            {
                var item = kvp.Value.item;
                var totalQty = kvp.Value.quantity;
                var maxStack = item.MaxStack;

                while (totalQty > 0)
                {
                    var placeAmount = Mathf.Min(totalQty, maxStack);
                    compactStacks.Add(new ItemStack(item, placeAmount));
                    totalQty -= placeAmount;
                }
            }

            compactStacks.AddRange(nonStackableList);

            // Step 3: Sort stacks deterministically
            compactStacks.Sort((a, b) =>
            {
                if (a.Item == null && b.Item == null) return 0;
                if (a.Item == null) return 1;
                if (b.Item == null) return -1;

                // Category order: Resource (0), Material (1), Consumable (2), Tool (3), Weapon (4), Armor (5), Backpack (6), Quest (7)...
                var catCompare = ((int)a.Item.Category).CompareTo((int)b.Item.Category);
                if (catCompare != 0) return catCompare;

                // Rarity descending: Legendary -> Common
                var rarityCompare = ((int)b.Item.Rarity).CompareTo((int)a.Item.Rarity);
                if (rarityCompare != 0) return rarityCompare;

                // DisplayName ascending
                var nameCompare = string.Compare(a.Item.DisplayName, b.Item.DisplayName, StringComparison.OrdinalIgnoreCase);
                if (nameCompare != 0) return nameCompare;

                // Quantity descending
                return b.Quantity.CompareTo(a.Quantity);
            });

            // Step 4: Write back into slots
            var time = Time.time;
            for (var i = 0; i < _slots.Count; i++)
            {
                if (i < compactStacks.Count)
                {
                    _slots[i] = compactStacks[i];
                }
                else
                {
                    _slots[i] = new ItemStack();
                }

                SlotChanged?.Invoke(new InventorySlotChangedEvent(_containerId, i, _slots[i], time));
            }

            PublishInventoryChanged();
        }

        public bool ReceiveItem(ItemDefinition item, int amount)
        {
            if (item == null || amount <= 0)
            {
                return false;
            }

            return AddItem(item, amount) > 0;
        }

        private void InitializeSlots(int count)
        {
            if (_slots == null)
            {
                _slots = new List<ItemStack>();
            }

            while (_slots.Count < count)
            {
                _slots.Add(new ItemStack());
            }
        }

        private void CheckEncumbranceChange()
        {
            var currentState = IsOverencumbered;
            if (currentState != _lastEncumberedState)
            {
                _lastEncumberedState = currentState;
                EncumbranceChanged?.Invoke(new InventoryEncumbranceChangedEvent(
                    _containerId,
                    currentState,
                    CurrentWeight,
                    _maxWeight,
                    Time.time));
            }
        }

        private void PublishInventoryChanged()
        {
            InventoryChanged?.Invoke(new InventoryChangedEvent(
                _containerId,
                CurrentWeight,
                _maxWeight,
                TotalItemCount,
                Time.time));

            OnInventoryChanged?.Invoke();
        }
    }
}
