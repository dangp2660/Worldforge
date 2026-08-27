using System;
using UnityEngine;

namespace Worldforge.Item
{
    /// <summary>
    /// Runtime state container representing a stack or instance of an item in the inventory system.
    /// </summary>
    [Serializable]
    public sealed class ItemStack : IInventoryItem
    {
        [SerializeField] private ItemDefinition _item;
        [SerializeField] private int _quantity;
        [SerializeField] private float _currentDurability;
        [SerializeField] private string _customName = string.Empty;
        [SerializeField] private bool _isLocked;
        [SerializeField] private bool _isRotated;

        private Guid _instanceId;

        public ItemStack()
        {
            _instanceId = Guid.NewGuid();
        }

        public ItemStack(ItemDefinition item, int quantity = 1, float currentDurability = 100f, string customName = "")
        {
            _instanceId = Guid.NewGuid();
            _item = item;
            _quantity = item != null ? Mathf.Clamp(quantity, 0, item.MaxStack) : 0;
            _currentDurability = currentDurability;
            _customName = customName ?? string.Empty;
        }

        public Guid InstanceId
        {
            get { return _instanceId; }
        }

        public ItemDefinition Item
        {
            get { return _item; }
        }

        public ItemDefinition Definition
        {
            get { return _item; }
        }

        public int Quantity
        {
            get { return _quantity; }
            set
            {
                if (_item == null)
                {
                    _quantity = 0;
                    return;
                }

                _quantity = Mathf.Clamp(value, 0, _item.MaxStack);
            }
        }

        public float CurrentDurability
        {
            get { return _currentDurability; }
            set { _currentDurability = Mathf.Max(0f, value); }
        }

        public string CustomName
        {
            get { return _customName; }
            set { _customName = value ?? string.Empty; }
        }

        public bool IsLocked
        {
            get { return _isLocked; }
            set { _isLocked = value; }
        }

        public bool IsRotated
        {
            get { return _isRotated; }
            set { _isRotated = value; }
        }

        public float TotalWeight
        {
            get { return _item != null ? _item.TotalWeightFor(_quantity) : 0f; }
        }

        public Vector2Int GridSize
        {
            get { return _item != null ? _item.GetRotatedGridSize(_isRotated) : Vector2Int.zero; }
        }

        public bool IsEmpty
        {
            get { return _item == null || _quantity <= 0; }
        }

        public bool IsFull
        {
            get { return _item != null && _quantity >= _item.MaxStack; }
        }

        public int AvailableSpace
        {
            get
            {
                if (_item == null)
                {
                    return 0;
                }

                return Mathf.Max(0, _item.MaxStack - _quantity);
            }
        }

        public bool CanStackWith(ItemDefinition item)
        {
            if (_item == null || item == null)
            {
                return false;
            }

            return _item.CanStackWith(item) && !IsFull;
        }

        public bool CanStackWith(ItemStack other)
        {
            if (other == null || other.IsEmpty || _item == null)
            {
                return false;
            }

            return _item.CanStackWith(other._item) && !IsFull;
        }

        public int Add(int amount, out int overflow)
        {
            if (_item == null || amount <= 0)
            {
                overflow = amount;
                return 0;
            }

            var space = AvailableSpace;
            var added = Mathf.Min(space, amount);
            _quantity += added;
            overflow = amount - added;
            return added;
        }

        public int Remove(int amount, out int remainder)
        {
            if (_item == null || amount <= 0)
            {
                remainder = 0;
                return 0;
            }

            var removed = Mathf.Min(_quantity, amount);
            _quantity -= removed;
            remainder = _quantity;

            if (_quantity <= 0)
            {
                Clear();
            }

            return removed;
        }

        public void Rotate()
        {
            _isRotated = !_isRotated;
        }

        public void Clear()
        {
            _item = null;
            _quantity = 0;
            _currentDurability = 0f;
            _customName = string.Empty;
            _isLocked = false;
            _isRotated = false;
        }
    }
}
