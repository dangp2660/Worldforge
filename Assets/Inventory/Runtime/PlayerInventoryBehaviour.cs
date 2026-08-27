using System;
using System.Collections.Generic;
using UnityEngine;
using Worldforge.Item;

namespace Worldforge.Inventory
{
    /// <summary>
    /// Player inventory MonoBehaviour component integrating the player with inventory services and gathered item reception.
    /// Implements <see cref="IGatheredItemReceiver"/> to accept harvested resources from gathering nodes.
    /// </summary>
    [DisallowMultipleComponent]
    [AddComponentMenu("Worldforge/Inventory/Player Inventory Behaviour")]
    public sealed class PlayerInventoryBehaviour : MonoBehaviour, IGatheredItemReceiver
    {
        [Header("Template Definition (Optional)")]
        [SerializeField] private InventoryDefinition _definition;

        [Header("Container Configuration")]
        [SerializeField] private string _containerId = "PlayerInventory";
        [SerializeField, Range(1, 100)] private int _initialSlotCount = 20;
        [SerializeField, Min(1f)] private float _maxWeightLimit = 50f;

        [Header("Starting Items (Custom fallback when no Definition)")]
        [SerializeField] private List<StartingItemEntry> _startingItems = new();

        [Header("Debug / Live Stats (Inspector View)")]
        [SerializeField] private float _currentWeightDebug;
        [SerializeField] private int _occupiedSlotsDebug;
        [SerializeField] private int _totalItemsDebug;
        [SerializeField] private bool _isOverencumberedDebug;
        [SerializeField] private List<string> _activeItemsSummary = new();

        [Header("Runtime State")]
        [SerializeField] private InventoryContainer _container;

        public InventoryContainer Container
        {
            get { return _container; }
        }

        public string ContainerId
        {
            get { return _containerId; }
        }

        public int SlotCount
        {
            get { return _container != null ? _container.SlotCount : _initialSlotCount; }
        }

        public float CurrentWeight
        {
            get { return _container != null ? _container.CurrentWeight : 0f; }
        }

        public float MaxWeight
        {
            get { return _container != null ? _container.MaxWeight : _maxWeightLimit; }
        }

        public bool IsOverencumbered
        {
            get { return _container != null && _container.IsOverencumbered; }
        }

        public int TotalItemCount
        {
            get { return _container != null ? _container.TotalItemCount : 0; }
        }

        public int EmptySlotCount
        {
            get { return _container != null ? _container.EmptySlotCount : _initialSlotCount; }
        }

        private void Awake()
        {
            InitializeContainer();
        }

        private void Update()
        {
            if (_container != null && Application.isPlaying)
            {
                UpdateDebugFields();
            }
        }

        public void InitializeContainer()
        {
            if (_container == null || _container.SlotCount == 0)
            {
                if (_definition == null)
                {
                    _definition = Resources.Load<InventoryDefinition>("Definitions/Inventory/Inventory_PlayerDefault");
                }

                if (_definition != null)
                {
                    _containerId = string.IsNullOrWhiteSpace(_containerId) || _containerId == "PlayerInventory"
                        ? _definition.InventoryCode
                        : _containerId;
                    _initialSlotCount = _definition.SlotCount;
                    _maxWeightLimit = _definition.WeightLimit;
                    _container = new InventoryContainer(_definition, _containerId);
                }
                else
                {
                    _container = new InventoryContainer(_containerId, _initialSlotCount, _maxWeightLimit);

                    if (_startingItems != null && _startingItems.Count > 0)
                    {
                        for (var i = 0; i < _startingItems.Count; i++)
                        {
                            var entry = _startingItems[i];
                            if (entry.Item != null && entry.Amount > 0)
                            {
                                _container.AddItem(entry.Item, entry.Amount);
                            }
                        }
                    }
                }

                if (_container != null)
                {
                    _container.ItemAdded += _ => UpdateDebugFields();
                    _container.ItemRemoved += _ => UpdateDebugFields();
                    _container.SlotChanged += _ => UpdateDebugFields();
                }

                UpdateDebugFields();
            }
        }

        public int AddItem(ItemDefinition item, int amount)
        {
            if (_container == null)
            {
                InitializeContainer();
            }

            var added = _container.AddItem(item, amount);
            UpdateDebugFields();
            return added;
        }

        public bool RemoveItem(ItemDefinition item, int amount)
        {
            if (_container == null)
            {
                return false;
            }

            var removed = _container.RemoveItem(item, amount);
            UpdateDebugFields();
            return removed;
        }

        public int GetItemCount(ItemDefinition item)
        {
            return _container != null ? _container.GetItemCount(item) : 0;
        }

        public void AutoSort()
        {
            _container?.AutoSort();
            UpdateDebugFields();
        }

        /// <summary>
        /// Receives items yielded from gathering nodes and inserts them into player inventory.
        /// </summary>
        public bool ReceiveItem(ItemDefinition item, int amount)
        {
            if (item == null || amount <= 0)
            {
                return false;
            }

            if (_container == null)
            {
                InitializeContainer();
            }

            var accepted = _container.AddItem(item, amount);
            UpdateDebugFields();

            if (accepted > 0)
            {
                Debug.Log(
                    $"[PlayerInventory] Received {accepted}x '{item.DisplayName}'. " +
                    $"Total in bag: {_container.GetItemCount(item)}. " +
                    $"Weight: {_container.CurrentWeight:F1}/{_container.MaxWeight:F1}kg.");
                return true;
            }

            Debug.LogWarning(
                $"[PlayerInventory] Could not receive '{item.DisplayName}' (Inventory full or overweight).");
            return false;
        }

        private void UpdateDebugFields()
        {
            if (_container == null)
            {
                return;
            }

            _currentWeightDebug = (float)Math.Round(_container.CurrentWeight, 2);
            _occupiedSlotsDebug = _container.SlotCount - _container.EmptySlotCount;
            _totalItemsDebug = _container.TotalItemCount;
            _isOverencumberedDebug = _container.IsOverencumbered;

            _activeItemsSummary.Clear();
            for (var i = 0; i < _container.SlotCount; i++)
            {
                var slot = _container.GetSlot(i);
                if (slot != null && !slot.IsEmpty && slot.Item != null)
                {
                    _activeItemsSummary.Add(
                        $"Slot [{i:D2}]: {slot.Item.DisplayName} x{slot.Quantity} ({slot.TotalWeight:F1}kg, {slot.Item.Category})");
                }
            }
        }
    }
}
