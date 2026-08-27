using System;
using System.Collections.Generic;
using UnityEngine;
using Worldforge.Item;

namespace Worldforge.Inventory
{
    /// <summary>
    /// Core container implementation managing item slots, stacking logic, and weight tracking.
    /// Implements <see cref="IInventoryContainer"/> and <see cref="IGatheredItemReceiver"/>.
    /// </summary>
    [Serializable]
    public sealed class InventoryContainer : IInventoryContainer, IGatheredItemReceiver
    {
        [SerializeField] private string _containerId = "PlayerInventory";
        [SerializeField] private int _slotCount = 20;
        [SerializeField] private float _maxWeight = 50f;
        [SerializeField] private List<ItemStack> _slots = new();

        public event Action OnInventoryChanged;

        public InventoryContainer()
        {
            InitializeSlots(_slotCount);
        }

        public InventoryContainer(string containerId, int slotCount = 20, float maxWeight = 50f)
        {
            _containerId = string.IsNullOrWhiteSpace(containerId) ? Guid.NewGuid().ToString() : containerId;
            _slotCount = Mathf.Max(1, slotCount);
            _maxWeight = Mathf.Max(1f, maxWeight);
            InitializeSlots(_slotCount);
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
                    if (_slots[i] != null && !_slots[i].IsEmpty)
                    {
                        total += _slots[i].TotalWeight;
                    }
                }
                return total;
            }
        }

        public float MaxWeight
        {
            get { return _maxWeight; }
            set { _maxWeight = Mathf.Max(1f, value); }
        }

        public bool IsOverencumbered
        {
            get { return CurrentWeight > _maxWeight; }
        }

        public void Resize(int newSlotCount)
        {
            _slotCount = Mathf.Max(1, newSlotCount);
            while (_slots.Count < _slotCount)
            {
                _slots.Add(new ItemStack());
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

        public int AddItem(ItemDefinition item, int amount)
        {
            if (item == null || amount <= 0)
            {
                return 0;
            }

            var remaining = amount;
            var totalAdded = 0;

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
                        var added = slot.Add(remaining, out remaining);
                        totalAdded += added;
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
                    }
                }
            }

            if (totalAdded > 0)
            {
                OnInventoryChanged?.Invoke();
            }

            return totalAdded;
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
            for (var i = _slots.Count - 1; i >= 0; i--)
            {
                if (remaining <= 0)
                {
                    break;
                }

                var slot = _slots[i];
                if (slot != null && !slot.IsEmpty && slot.Item == item)
                {
                    var removed = slot.Remove(remaining, out _);
                    remaining -= removed;
                }
            }

            OnInventoryChanged?.Invoke();
            return true;
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
                if (slot != null && !slot.IsEmpty && slot.Item == item)
                {
                    count += slot.Quantity;
                }
            }

            return count;
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
    }
}
