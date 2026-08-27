using System;
using System.Collections.Generic;
using UnityEngine;
using Worldforge.Item;

namespace Worldforge.Inventory
{
    /// <summary>
    /// Configuration entry for a starting item placed into an inventory container upon initialization.
    /// </summary>
    [Serializable]
    public struct StartingItemEntry
    {
        [SerializeField] private ItemDefinition _item;
        [SerializeField, Min(1)] private int _amount;

        public StartingItemEntry(ItemDefinition item, int amount)
        {
            _item = item;
            _amount = Mathf.Max(1, amount);
        }

        public ItemDefinition Item
        {
            get { return _item; }
        }

        public int Amount
        {
            get { return _amount; }
        }
    }

    /// <summary>
    /// ScriptableObject defining baseline specifications for an inventory container template.
    /// Corresponds to Schema Part 4: InventoryDefinition.
    /// </summary>
    [CreateAssetMenu(
        fileName = "InventoryDefinition",
        menuName = "Worldforge/Inventory/Inventory Definition")]
    public sealed class InventoryDefinition : ScriptableObject
    {
        [Header("Identity")]
        [SerializeField] private string _inventoryCode = "INV_DEFAULT";
        [SerializeField] private string _displayName = "Default Inventory";
        [SerializeField, TextArea(2, 4)] private string _description = string.Empty;

        [Header("Capacity & Weight")]
        [SerializeField, Range(1, 100)] private int _slotCount = 20;
        [SerializeField, Min(1f)] private float _weightLimit = 50f;

        [Header("Container Rules")]
        [SerializeField] private bool _allowSort = true;
        [SerializeField] private bool _allowStack = true;
        [SerializeField] private bool _allowQuickMove = true;

        [Header("Starting Items")]
        [SerializeField] private List<StartingItemEntry> _startingItems = new();

        public string InventoryCode
        {
            get { return _inventoryCode; }
        }

        public string DisplayName
        {
            get { return _displayName; }
        }

        public string Description
        {
            get { return _description; }
        }

        public int SlotCount
        {
            get { return _slotCount; }
        }

        public float WeightLimit
        {
            get { return _weightLimit; }
        }

        public bool AllowSort
        {
            get { return _allowSort; }
        }

        public bool AllowStack
        {
            get { return _allowStack; }
        }

        public bool AllowQuickMove
        {
            get { return _allowQuickMove; }
        }

        public IReadOnlyList<StartingItemEntry> StartingItems
        {
            get { return _startingItems; }
        }

        private void OnValidate()
        {
            _slotCount = Mathf.Clamp(_slotCount, 1, 100);
            _weightLimit = Mathf.Max(1f, _weightLimit);
        }
    }
}
